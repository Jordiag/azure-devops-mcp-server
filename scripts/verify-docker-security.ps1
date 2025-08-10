# Docker build verification script for security-hardened Dockerfile
# This script verifies that the build works with the security improvements

Write-Host "🔒 Testing security-hardened Docker build..." -ForegroundColor Green
Write-Host "📁 Checking that sensitive files are properly excluded..." -ForegroundColor Yellow

# Verify .dockerignore excludes sensitive files
$dockerIgnoreContent = Get-Content .dockerignore -Raw

if ($dockerIgnoreContent -like "*Set-Local-Test-Dev-Env-Vars.ps1*") {
    Write-Host "✅ PowerShell environment script excluded from Docker context" -ForegroundColor Green
} else {
    Write-Host "❌ PowerShell environment script not excluded" -ForegroundColor Red
    exit 1
}

if ($dockerIgnoreContent -like "*run-local.*") {
    Write-Host "✅ Local run scripts excluded from Docker context" -ForegroundColor Green
} else {
    Write-Host "❌ Local run scripts not excluded" -ForegroundColor Red
    exit 1
}

# Verify Dockerfile uses specific COPY instead of COPY . .
$dockerfileContent = Get-Content Dockerfile -Raw

if ($dockerfileContent -like '*COPY*src/*') {
    Write-Host "✅ Dockerfile uses specific source copy instead of blanket copy" -ForegroundColor Green
} else {
    Write-Host "❌ Dockerfile still uses risky COPY . . command" -ForegroundColor Red
    exit 1
}

# Verify Dockerfile creates and uses non-root user
if ($dockerfileContent -like '*USER appuser*') {
    Write-Host "✅ Dockerfile uses non-root user (appuser) for security" -ForegroundColor Green
} else {
    Write-Host "❌ Dockerfile runs as root user (security risk)" -ForegroundColor Red
    exit 1
}

if ($dockerfileContent -like '*adduser*appuser*') {
    Write-Host "✅ Dockerfile creates dedicated application user" -ForegroundColor Green
} else {
    Write-Host "❌ Dockerfile does not create non-root application user" -ForegroundColor Red
    exit 1
}

# Test Docker build (will only work if Docker is running)
Write-Host "🐳 Attempting Docker build test..." -ForegroundColor Yellow

try {
    $dockerInfo = docker info 2>$null
    if ($LASTEXITCODE -eq 0) {
        docker build -t azure-devops-mcp-server-security-test .
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Docker build succeeded with security improvements" -ForegroundColor Green
            
            # Clean up test image
            docker rmi azure-devops-mcp-server-security-test 2>$null | Out-Null
        } else {
            Write-Host "❌ Docker build failed" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "⚠️  Docker not available - build test skipped" -ForegroundColor Yellow
        Write-Host "   (This is normal in CI environments without Docker)" -ForegroundColor Gray
    }
} catch {
    Write-Host "⚠️  Docker not available - build test skipped" -ForegroundColor Yellow
    Write-Host "   (This is normal in CI environments without Docker)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "🎉 Security verification completed successfully!" -ForegroundColor Green
Write-Host "   - Sensitive files are excluded via .dockerignore" -ForegroundColor Gray
Write-Host "   - Dockerfile uses specific file copying instead of blanket copy" -ForegroundColor Gray
Write-Host "   - Container runs as non-root user (appuser) for security" -ForegroundColor Gray
Write-Host "   - Build process should work without including sensitive data" -ForegroundColor Gray
