#!/bin/bash
# Docker build verification script for security-hardened Dockerfile
# This script verifies that the build works with the security improvements

echo "🔒 Testing security-hardened Docker build..."
echo "📁 Checking that sensitive files are properly excluded..."

# Verify .dockerignore excludes sensitive files
if grep -q "Set-Local-Test-Dev-Env-Vars.ps1" .dockerignore; then
    echo "✅ PowerShell environment script excluded from Docker context"
else
    echo "❌ PowerShell environment script not excluded"
    exit 1
fi

if grep -q "run-local.*" .dockerignore; then
    echo "✅ Local run scripts excluded from Docker context"
else
    echo "❌ Local run scripts not excluded"
    exit 1
fi

# Verify Dockerfile uses specific COPY instead of COPY . .
if grep -q "COPY \[\"src/\", \"src/\"\]" Dockerfile; then
    echo "✅ Dockerfile uses specific source copy instead of blanket copy"
else
    echo "❌ Dockerfile still uses risky COPY . . command"
    exit 1
fi

# Verify Dockerfile creates and uses non-root user
if grep -q "USER appuser" Dockerfile; then
    echo "✅ Dockerfile uses non-root user (appuser) for security"
else
    echo "❌ Dockerfile runs as root user (security risk)"
    exit 1
fi

if grep -q "adduser.*appuser" Dockerfile; then
    echo "✅ Dockerfile creates dedicated application user"
else
    echo "❌ Dockerfile does not create non-root application user"
    exit 1
fi

# Test Docker build (will only work if Docker is running)
echo "🐳 Attempting Docker build test..."
if command -v docker > /dev/null && docker info > /dev/null 2>&1; then
    docker build -t azure-devops-mcp-server-security-test . || {
        echo "❌ Docker build failed"
        exit 1
    }
    echo "✅ Docker build succeeded with security improvements"
    
    # Clean up test image
    docker rmi azure-devops-mcp-server-security-test > /dev/null 2>&1 || true
else
    echo "⚠️  Docker not available - build test skipped"
    echo "   (This is normal in CI environments without Docker)"
fi

echo ""
echo "🎉 Security verification completed successfully!"
echo "   - Sensitive files are excluded via .dockerignore"
echo "   - Dockerfile uses specific file copying instead of blanket copy"
echo "   - Container runs as non-root user (appuser) for security"  
echo "   - Build process should work without including sensitive data"
