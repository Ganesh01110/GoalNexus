# ⚡ Hybrid Serverless Architecture Guide

> How to deploy GoalNexus to **AWS Lambda** (Serverless) instead of App Runner. 
> This approach costs exactly $0.00 to run and natively supports ASP.NET Minimal APIs.

---

## 1. 🌉 The "Hybrid Bridge" Concept

In traditional .NET, an application relies on a built-in web server called **Kestrel** (which `dotnet run` starts). 
AWS Lambda, however, doesn't use web servers. It only runs code in short bursts when an HTTP event happens.

We installed the `Amazon.Lambda.AspNetCoreServer.Hosting` NuGet package and added this line to `Program.cs`:
```csharp
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
```

**What this does:**
1. If you run the app locally, it ignores the bridge and works normally.
2. If the app is launched inside AWS Lambda, it intercepts the "Lambda Event," bypasses Kestrel, and feeds the event directly to your ASP.NET endpoints. 
**Result:** 100% code reuse!

---

## 2. 🔐 How the Backend Authenticates to DynamoDB

In the cloud, you **never hardcode AWS Access Keys**. 

1. Every AWS Lambda function must be assigned an **Execution Role** (an IAM Role).
2. The AWS SDK in your `.cs` files automatically detects that it is running inside Lambda.
3. It asks AWS for temporary credentials based *only* on that Execution Role.
4. Because the Execution Role has the `AmazonDynamoDBFullAccess` policy attached, the code seamlessly saves data.

---

## 3. 🚀 Manual Deployment Steps for AWS Lambda

Since App Runner is currently blocking you, here is the exact manual path to deploy this to AWS Lambda.

### Step A: Publish the Code
In your backend terminal (`cd backend/GoalNexus.Api`), run:
```bash
dotnet publish -c Release -r linux-x64 --self-contained false -p:PublishReadyToRun=true -o ./publish
```
Then, compress the contents of that `publish` folder into a zip file named `api.zip`. 

### Step B: Create the IAM Execution Role
1. Log into the AWS Console -> **IAM** -> **Roles** -> **Create Role**.
2. Select **AWS Service** -> Choose **Lambda** and click Next.
3. Search for and check these two permissions:
   - `AWSLambdaBasicExecutionRole` (Allows it to write console logs)
   - `AmazonDynamoDBFullAccess` (Allows it to talk to your Terraform database)
4. Name it `GoalNexusLambdaRole` and click Create.

### Step C: Create the Lambda Function
1. Go to the **AWS Lambda Console** -> **Create Function**.
2. Select **Author from scratch**.
3. Name: `GoalNexusAPI`
4. Runtime: **.NET 8 (C#) / .NET 9** (whichever matches your `csproj`).
5. Architecture: **x86_64**
6. permissions -> **Change default execution role** -> **Use an existing role** -> Select `GoalNexusLambdaRole`.
7. Click **Create function**.

### Step D: Upload Code and Configure Handlers
1. In the Lambda Function page, click **Upload from** -> **.zip file** and upload your `api.zip`.
2. Scroll down to **Runtime settings** and click **Edit**.
3. **Handler**: This must exactly match your Assembly Name. Change it to: `GoalNexus.Api`
4. Save.

### Step E: Add Environment Variables
1. Go to the **Configuration** tab -> **Environment variables** -> **Edit**.
2. Add:
   - Key: `AWS__Region` | Value: `ap-south-1`
   - Key: `ASPNETCORE_ENVIRONMENT` | Value: `Production`
3. Save.

### Step F: Expose to the Internet via API Gateway
*Lambda functions are hidden by default. You need a URL to reach them.*

1. In your Lambda page, click **+ Add trigger**.
2. Select **API Gateway**.
3. Intent: **Create a new API**
4. API type: **HTTP API**
5. Security: **Open**
6. Click **Add**.

AWS will immediately give you an **API endpoint URL**. 

Take that exact URL, paste it into your Vercel Environment Variables as `VITE_API_BASE_URL`, and deploy! Your full-stack, serverless app is now live!
