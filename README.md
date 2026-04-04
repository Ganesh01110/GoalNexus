# GoalNexus: Modern Full-Stack Goal Tracker

GoalNexus is a minimalistic, premium web application designed to help you track your personal goals without the friction of account creation. It showcases how to integrate **ASP.NET Core Minimal APIs** with **React (Tailwind)** and **AWS DynamoDB**.

## 🚀 How it Works (Privacy First)
GoalNexus uses an **Anonymous ID** system. When you visit the site, a unique identifier is generated and saved in your browser's local storage.
- **Privacy**: Your goals are linked to your browser's ID.
- **Sharing**: If you share the link, others see their own empty dashboard (they get their own ID).
- **Simplicity**: No passwords, no emails, just goals.

---

## 🛠️ Backend Deep Dive (ASP.NET Core)

The backend is built using the modern **Minimal APIs** pattern in .NET, which reduces boilerplate code and focuses on performance.

### File Structure & Roles
- `Program.cs`: The "Brain". It configures services (AWS, CORS, Dependency Injection) and defines the API endpoints (Routes).
- `Models/Goal.cs`: The "Blueprint". Defines what a Goal looks like (UserId, GoalId, Title, etc.) and matches the DynamoDB schema.
- `Services/GoalService.cs`: The "Worker". Handles all database logic (Save, Delete, Update) using the AWS SDK for DynamoDB.
- `appsettings.json`: The "Secretary". Stores configuration like AWS region (default: us-east-1).
- `Properties/launchSettings.json`: The "Map". Defines which port the app runs on (Default: 5136).

### Data Flow
1. **Request**: Frontend sends a JSON request (e.g., `POST /api/goals`) to port `5136`.
2. **Middleware**: ASP.NET checks for **CORS** permission (allowing React to talk to it) and logs the request.
3. **Endpoint**: `Program.cs` matches the route and hands the data to `IGoalService`.
4. **Service**: `GoalService` uses the **DynamoDB Context** to save the C# object directly into the AWS cloud.
5. **Response**: A Success/Error message is sent back to the React frontend.

---

## ⚙️ How to Run & Verify Locally

### 1. Cloud Infrastructure (One-time)
Ensure you have AWS credentials set up, then provision the table:
```bash
cd terraform
terraform init
terraform apply
```

### 2. Backend Verification
```bash
cd backend/GoalNexus.Api
dotnet run
```
**To verify**: Open `http://localhost:5136/swagger` in your browser. You can test the API directly from this visual interface!

### 3. Frontend Selection
```bash
cd frontend
npm install
npm run dev
```
**To verify**: Open the URL shown in your terminal (usually `http://localhost:5173`). Add a goal and check your terminal logs—you'll see the backend logging the interaction!

---

---

## 🏗️ Infrastructure Management (Terraform)

Terraform allows you to manage your AWS resources as code.

- **Deploy**: `terraform init` followed by `terraform apply` to build the DynamoDB table.
- **Tear Down**: When you're done or want to avoid any potential costs (though DynamoDB has an 25GB free tier), use:
```bash
terraform destroy
```

---

## 🔐 GitHub Configuration (Secrets)

To enable the **GitHub Action** (CI/CD), you must add your AWS credentials as "Secrets" in your repository settings:

1. Go to **Settings** > **Secrets and variables** > **Actions**.
2. Add the following **Repository secrets**:
   - `AWS_ACCESS_KEY_ID`: Your AWS Access Key.
   - `AWS_SECRET_ACCESS_KEY`: Your AWS Secret Key.
   - `AWS_REGION`: `us-east-1` (or your preferred region).


