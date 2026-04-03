provider "aws" {
  region = "us-east-1"
}

# DynamoDB Table for Goals
resource "aws_dynamodb_table" "goals_table" {
  name           = "GoalNexus_Goals"
  billing_mode   = "PROVISIONED"
  read_capacity  = 5
  write_capacity = 5
  hash_key       = "UserId"
  range_key      = "GoalId"

  attribute {
    name = "UserId"
    type = "S"
  }

  attribute {
    name = "GoalId"
    type = "S"
  }

  tags = {
    Project     = "GoalNexus"
    Environment = "Dev"
  }
}

# Output the table name
output "dynamodb_table_name" {
  value = aws_dynamodb_table.goals_table.name
}

# Output the region
output "aws_region" {
  value = "us-east-1"
}
