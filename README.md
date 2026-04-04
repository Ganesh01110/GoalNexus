# 🎯 GoalNexus — Anonymous Full-Stack Goal Tracker

> A minimalistic, privacy-first goal tracking web app. No accounts, no passwords — just your goals.

Built with **ASP.NET Core Minimal APIs** · **React + Tailwind CSS v4** · **AWS DynamoDB** · **Terraform**

---

## ✨ Features

- 🔒 **Anonymous by Design** — A unique Secret ID is generated in your browser. No sign-up required.
- 🌐 **Cloud-Native Storage** — Goals are persisted in AWS DynamoDB, available across sessions.
- ⚡ **Real-time CRUD** — Add, complete, and delete goals instantly.
- 🏗️ **Infrastructure as Code** — One command (`terraform apply`) provisions everything.
- 🔄 **CI/CD Pipeline** — GitHub Actions validates Backend, Frontend, and Terraform in parallel.

---

## 🚀 How It Works (Privacy Model)

GoalNexus uses an **Anonymous ID** system:

1. **First Visit** → A unique UUID is generated and saved in your browser's `localStorage`.
2. **Your Data** → Every goal you create is linked to that UUID in DynamoDB.
3. **Sharing the Link** → If someone else opens the same URL, they get their **own** UUID and see an empty dashboard.
4. **Result** → Complete data isolation without authentication.

---

## 🛠️ Tech Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Frontend** | React 19, Tailwind CSS v4, Vite 8 | Modern SPA with hot-reload |
| **Backend** | ASP.NET Core 9 (Minimal APIs) | RESTful API with Swagger docs |
| **Database** | AWS DynamoDB | Serverless NoSQL storage |
| **Infra** | Terraform | Infrastructure as Code |
| **CI/CD** | GitHub Actions | Parallel build validation |

---

## 📂 Backend Deep Dive (ASP.NET Core)

The backend uses the modern **Minimal APIs** pattern — no controllers, no boilerplate. Everything is explicit and direct.

### File Structure & Roles

```
backend/GoalNexus.Api/
├── Program.cs                    # App entry point: service config + API route definitions
├── Models/
│   └── Goal.cs                   # Data model with DynamoDB table/key annotations
├── Services/
│   └── GoalService.cs            # Business logic: all DynamoDB CRUD operations
├── Properties/
│   └── launchSettings.json       # Dev server config (port: 5136)
├── appsettings.json              # App config (AWS region, logging levels)
└── GoalNexus.Api.csproj          # Project dependencies (AWS SDK, Swashbuckle)
```

### How Each File Works

| File | Role | What It Does |
|------|------|-------------|
| `Program.cs` | **The Brain** | Registers AWS services, configures CORS, defines all 4 REST endpoints, enables Swagger UI |
| `Goal.cs` | **The Blueprint** | C# class annotated with `[DynamoDBTable]`, `[DynamoDBHashKey]`, `[DynamoDBRangeKey]` to map directly to DynamoDB |
| `GoalService.cs` | **The Worker** | Uses `DynamoDBContext` with explicit `OverrideTableName` config for reliable table access |
| `appsettings.json` | **The Config** | Stores the AWS region (`ap-south-1`) so the SDK knows which datacenter to connect to |
| `launchSettings.json` | **The Map** | Tells `dotnet run` to serve the API on `http://localhost:5136` |

### API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/goals/{userId}` | Fetch all goals for a user |
| `POST` | `/api/goals` | Create a new goal |
| `PATCH` | `/api/goals/{userId}/{goalId}/toggle` | Toggle completion status |
| `DELETE` | `/api/goals/{userId}/{goalId}` | Permanently delete a goal |

### Data Flow

```
Browser (React)
    │
    ▼
POST /api/goals  ──►  Program.cs (Route Matching)
                           │
                           ▼
                      GoalService.cs (Business Logic)
                           │
                           ▼
                      AWS DynamoDB (Cloud Storage)
                           │
                           ▼
                      Response ──►  Browser Updates UI
```

---

## ⚙️ Local Development Setup

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Terraform](https://www.terraform.io/downloads)
- [AWS CLI](https://aws.amazon.com/cli/) configured with `aws configure`

### Step 1: Provision the Database

```bash
cd terraform
terraform init
terraform apply     # Type 'yes' when prompted
```

### Step 2: Start the Backend

```bash
cd backend/GoalNexus.Api
dotnet run
```
> **Verify**: Open [http://localhost:5136/swagger](http://localhost:5136/swagger) — you should see the interactive API docs.
>
> **Pro Tip**: Use `dotnet watch run` instead for automatic hot-reload on code changes.

### Step 3: Start the Frontend

```bash
cd frontend
npm install         # First time only
npm run dev
```
> **Verify**: Open [http://localhost:5173](http://localhost:5173) — add a goal and watch the backend terminal log the interaction!

---

## 🏗️ Infrastructure Management (Terraform)

Terraform provisions and manages all AWS resources as code.

| Command | What It Does |
|---------|-------------|
| `terraform init` | Downloads the AWS provider plugin (one-time) |
| `terraform plan` | Preview what will be created/changed |
| `terraform apply` | Create the DynamoDB table in AWS |
| `terraform destroy` | **Tear down** all resources (removes the table to avoid costs) |

> **Cost**: DynamoDB has a generous **25GB Always-Free** tier. This project uses minimal capacity (5 RCU/WCU), so you should incur **zero cost** for personal use.

---

## 🔐 AWS Credentials Setup

### Where to Get Your AWS Keys

1. Log in to the [AWS Management Console](https://console.aws.amazon.com)
2. Click your **username** (top-right corner) → **Security credentials**
3. Scroll to **"Access keys"** section → Click **"Create access key"**
4. Select **"Command Line Interface (CLI)"** as the use case
5. **Copy** both values:
   - `Access Key ID` (looks like: `AKIAIOSFODNN7EXAMPLE`)
   - `Secret Access Key` (looks like: `wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY`)

> ⚠️ **Important**: The Secret Access Key is shown **only once**. Save it securely!

### Local Setup (AWS CLI)

Run this once on your machine:

```bash
aws configure
```

Enter your Access Key, Secret Key, and region (`ap-south-1`).

### GitHub Actions Setup (CI/CD)

To enable the automated pipeline:

1. Go to your GitHub repo → **Settings** → **Secrets and variables** → **Actions**
2. Click **"New repository secret"** and add these three:

| Secret Name | Value |
|-------------|-------|
| `AWS_ACCESS_KEY_ID` | Your Access Key ID |
| `AWS_SECRET_ACCESS_KEY` | Your Secret Access Key |
| `AWS_REGION` | `ap-south-1` |

---

## 🔄 CI/CD Pipeline

The GitHub Actions workflow (`.github/workflows/deploy.yml`) runs **3 parallel jobs** on every push to `main`:

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  Backend CI     │    │  Frontend CI    │    │  Terraform CI   │
│                 │    │                 │    │                 │
│  dotnet restore │    │  npm ci         │    │  terraform init │
│  dotnet build   │    │  npm run build  │    │  terraform      │
│                 │    │                 │    │    validate     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

This ensures broken code is caught **before** it reaches production.

---

## 📄 License

This project is open source and available for learning purposes.
