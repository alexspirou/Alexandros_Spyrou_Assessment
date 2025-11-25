# Run Novibet.Wallet locally

1. **Start dependencies** (SQL Server + Redis) from the repo root:

   ```powershell
   .\init-containers.ps1
   ```

2. **Create .env** at repo root:

   ```
   SA_PASSWORD=<your password>
   ```

3. **Add connection strings** (store in user secrets; replace placeholders):

   ```json
   {
     "ConnectionStrings": {
       "SqlServerConnectionString": "<your sql connection string>",
       "Redis": "<your redis connection string>"
     }
   }
   ```

4. **Run the API**:
   Start Novibet.Wallet.Api project
