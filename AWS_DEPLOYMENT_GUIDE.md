# 📘 GoalNexus — AWS Deployment & Operations Guide

> A comprehensive reference for running locally, deploying to AWS, and managing cloud resources.

---

## Table of Contents

1. [Local Development](#-1-local-development)
2. [CI/CD Pipeline & Its Role](#-2-cicd-pipeline--its-role)
3. [AWS Deployment Flow](#-3-aws-deployment-flow)
4. [Cost Breakdown & Risk Assessment](#-4-cost-breakdown--risk-assessment)
5. [Cleanup — Tear Down Everything](#-5-cleanup--tear-down-everything)

---

## 🖥️ 1. Local Development

### Prerequisites

| Tool | Purpose | Download |
|------|---------|----------|
| .NET 9 SDK | Run the backend API | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| Node.js 20+ | Run the frontend React app | [nodejs.org](https://nodejs.org/) |
| Terraform | Provision AWS infrastructure | [terraform.io](https://www.terraform.io/downloads) |
| AWS CLI | Authenticate with AWS from your machine | [aws.amazon.com/cli](https://aws.amazon.com/cli/) |

### One-Time Setup: AWS CLI

```bash
aws configure
```

You'll be prompted for:
- **Access Key ID**: (from AWS Console → Security Credentials)
- **Secret Access Key**: (shown only once when created)
- **Default region**: `ap-south-1`
- **Output format**: `json`

### Running Locally (3 Terminals)

#### Terminal 1 — Provision Database (One-Time)

```bash
cd terraform
terraform init      # Download AWS provider (first time only)
terraform apply     # Creates DynamoDB table in ap-south-1
```

#### Terminal 2 — Start Backend API

```bash
cd backend/GoalNexus.Api
dotnet run          # Starts API on http://localhost:5136
```

> **Verify**: Open [http://localhost:5136/swagger](http://localhost:5136/swagger) — you should see interactive API docs.
>
> **Pro Tip**: Use `dotnet watch run` for automatic hot-reload when you edit C# files.

#### Terminal 3 — Start Frontend

```bash
cd frontend
npm install         # First time only
npm run dev         # Starts React on http://localhost:5173
```

> **Verify**: Open [http://localhost:5173](http://localhost:5173) — add a goal and check the backend terminal for logs.

### How the Pieces Connect Locally

```
┌──────────────────────────────────────────────────────────┐
│                    YOUR MACHINE                          │
│                                                          │
│  ┌─────────────┐         ┌─────────────────┐            │
│  │  React App  │ ──────► │ ASP.NET Core API│            │
│  │  :5173      │  HTTP   │  :5136          │            │
│  └─────────────┘         └────────┬────────┘            │
│                                   │                      │
└───────────────────────────────────┼──────────────────────┘
                                    │ AWS SDK
                                    ▼
                          ┌──────────────────┐
                          │  AWS DynamoDB    │
                          │  (ap-south-1)   │
                          │  Cloud ☁️        │
                          └──────────────────┘
```

---

## 🔄 2. CI/CD Pipeline & Its Role

### What It Does

The GitHub Actions workflow (`.github/workflows/deploy.yml`) runs **3 parallel jobs** every time you push to `main`:

```
                        ┌─────────────────────┐
                        │   git push origin   │
                        │       main          │
                        └──────────┬──────────┘
                                   │
                    ┌──────────────┼──────────────┐
                    ▼              ▼              ▼
          ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
          │ Backend CI  │ │ Frontend CI │ │Terraform CI │
          │             │ │             │ │             │
          │ dotnet      │ │ npm ci      │ │ terraform   │
          │  restore    │ │ npm run     │ │  init       │
          │ dotnet      │ │  build      │ │ terraform   │
          │  build      │ │             │ │  validate   │
          └──────┬──────┘ └──────┬──────┘ └──────┬──────┘
                 │               │               │
                 └───────────────┼───────────────┘
                                 ▼
                          ✅ All Pass or ❌ Fail
```

### What It Does NOT Do (Currently)

- ❌ Does **NOT** deploy to AWS automatically
- ❌ Does **NOT** start any cloud services
- ❌ Does **NOT** run `terraform apply`

It is purely a **"Quality Gate"** — it catches broken code before it reaches production.

### Why Parallel Jobs?

| Approach | Time | Debugging |
|----------|------|-----------|
| **Sequential** (one after another) | ~5 min | Hard to tell which step failed |
| **Parallel** (all at once) | ~2 min | Instantly see which job failed |

### GitHub Secrets Required

These are needed for the Terraform CI job to validate your `.tf` files:

| Secret Name | Where to Get It |
|-------------|----------------|
| `AWS_ACCESS_KEY_ID` | AWS Console → Your Username → Security Credentials → Access Keys |
| `AWS_SECRET_ACCESS_KEY` | Same page (shown only once at creation) |
| `AWS_REGION` | `ap-south-1` (or your preferred region) |

**How to add them:**

1. Go to your GitHub repo → **Settings** → **Secrets and variables** → **Actions**
2. Click **"New repository secret"**
3. Add each secret one by one

---

## ☁️ 3. AWS Deployment Flow

### The Big Picture

Deploying to the cloud is fundamentally different from running locally. There are **no terminals** to keep open — services run permanently until you delete them.

### Local vs Cloud Comparison

| What | Local (Your PC) | Cloud (AWS) |
|------|----------------|-------------|
| **Database** | `terraform apply` (one-time) | Same — `terraform apply` (one-time) |
| **Backend** | `dotnet run` (manual, stops when you close terminal) | **AWS App Runner** runs it 24/7 automatically |
| **Frontend** | `npm run dev` (manual, stops when you close terminal) | **Vercel / AWS S3** serves it permanently |
| **Connection** | `localhost:5136` | `https://xxxxx.awsapprunner.com` |

### Step-by-Step Cloud Deployment

#### Step 1: Database (Already Done)

You've already run `terraform apply` — the DynamoDB table exists in `ap-south-1`. This step is the same for local and cloud. ✅

#### Step 2: Backend — AWS App Runner (One-Time Manual Setup)

1. Go to [AWS App Runner Console](https://console.aws.amazon.com/apprunner)
2. Click **"Create service"**
3. Choose **"Source code repository"**
4. **Connect GitHub** → Authorize AWS → Select `Ganesh01110/GoalNexus`
5. **Configure build settings**:
   - **Runtime**: `.NET 9`
   - **Build command**: `cd backend/GoalNexus.Api && dotnet publish -c Release -o out`
   - **Start command**: `dotnet out/GoalNexus.Api.dll`
   - **Port**: `5136`
6. **Set Environment Variables**:
   - `AWS__Region` = `ap-south-1`
   - `ASPNETCORE_URLS` = `http://0.0.0.0:5136`
7. Click **Deploy**

App Runner will give you a public URL like:
```
https://abc123xyz.ap-south-1.awsapprunner.com
```

#### Step 3: Frontend — Hosting Options

| Option | Service | Cost | Complexity | Best For |
|--------|---------|------|------------|----------|
| **A** | Vercel | Always free | Very Easy | Learning / Portfolio |
| **B** | AWS S3 + CloudFront | Free tier | Medium | Full AWS experience |
| **C** | Netlify | Always free | Very Easy | Quick demos |

For any option, you need to update the `API_BASE` in `frontend/src/App.tsx`:
```typescript
// Change from:
const API_BASE = 'http://localhost:5136/api/goals'

// Change to:
const API_BASE = 'https://abc123xyz.ap-south-1.awsapprunner.com/api/goals'
```

#### After Setup — Automatic Flow

```
You push to GitHub
       │
       ├──► GitHub Actions: Validates code (CI/CD quality gate)
       │
       └──► AWS App Runner: Detects new commit on main
                   │
                   ▼
             Pulls latest code from GitHub
                   │
                   ▼
             Builds the .NET project
                   │
                   ▼
             Deploys and starts the API
                   │
                   ▼
             Live at your public URL 🌐
```

> **Key Point**: After the one-time setup, you just `git push` and everything updates automatically. No terminals, no manual steps.

---

## 💰 4. Cost Breakdown & Risk Assessment

### Service-by-Service Cost Analysis

| Service | Free Tier | Monthly Cost (After Free Tier) | Risk for This Project |
|---------|-----------|-------------------------------|----------------------|
| **DynamoDB** | 25GB storage + 25 RCU/WCU **forever** | ~$0 for small usage | ✅ **Zero risk** — this project uses <1MB |
| **App Runner** | Limited (first 12 months) | **~$5/month minimum** | ⚠️ **Watch this one** |
| **S3 (Frontend)** | 5GB storage + 20K requests | ~$0.03/month | ✅ Negligible |
| **Vercel (Frontend)** | Always free for personal | $0 | ✅ Best for learning |
| **GitHub Actions** | 2,000 min/month free | $0 for this project | ✅ Zero risk |

### The Smart Strategy

> **Deploy → Screenshot → Destroy**
>
> Total cost: **₹0** if you tear down within a few hours.

### What to Screenshot for Your Resume/Portfolio

1. ✅ The live app working at the App Runner URL
2. ✅ AWS Console showing your DynamoDB table with data
3. ✅ AWS Console showing your App Runner service running
4. ✅ GitHub Actions showing all 3 jobs passing green
5. ✅ Swagger UI accessible at the public URL

---

## 🧹 5. Cleanup — Tear Down Everything

> Run these steps when you're done experimenting. **Zero resources = zero bills.**

### Step 1: Delete App Runner Service

**Option A — AWS Console (Recommended):**

```
AWS Console → App Runner → Select your service → Actions → Delete
```

**Option B — AWS CLI:**

```bash
# List all App Runner services
aws apprunner list-services --region ap-south-1

# Delete the service (use the ARN from the list command)
aws apprunner delete-service --service-arn <your-service-arn> --region ap-south-1
```

### Step 2: Destroy DynamoDB Table

```bash
cd terraform
terraform destroy    # Type 'yes' when prompted
```

Expected output:
```
Destroy complete! Resources: 1 destroyed.
```

### Step 3: Verify Nothing Is Running

```bash
# Check DynamoDB — should show empty TableNames
aws dynamodb list-tables --region ap-south-1

# Check App Runner — should show empty ServiceSummaryList
aws apprunner list-services --region ap-south-1
```

### Step 4: Check AWS Billing (Peace of Mind)

1. Go to [AWS Billing Dashboard](https://console.aws.amazon.com/billing)
2. Check **"Bills"** → Current month
3. Everything should show **$0.00**

### Quick Cleanup Checklist

```
[ ] App Runner service deleted
[ ] DynamoDB table destroyed (terraform destroy)
[ ] Verified with list commands (both empty)
[ ] Checked billing dashboard ($0.00)
[ ] Frontend hosting removed (if using S3/Vercel)
```

---

## 📌 Quick Reference Commands

| Task | Command |
|------|---------|
| Configure AWS CLI | `aws configure` |
| Create database | `cd terraform && terraform init && terraform apply` |
| Destroy database | `cd terraform && terraform destroy` |
| Start backend (local) | `cd backend/GoalNexus.Api && dotnet run` |
| Start backend (hot-reload) | `cd backend/GoalNexus.Api && dotnet watch run` |
| Start frontend (local) | `cd frontend && npm run dev` |
| Check DynamoDB tables | `aws dynamodb list-tables --region ap-south-1` |
| Check App Runner services | `aws apprunner list-services --region ap-south-1` |
| Check AWS bill | [console.aws.amazon.com/billing](https://console.aws.amazon.com/billing) |

---

*Last updated: April 2026*
