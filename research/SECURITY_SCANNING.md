# Security Scanning

This document describes the security scanning setup for the AgenTerra project.

## Overview

The project implements multiple layers of security scanning to ensure code quality and identify vulnerabilities early in the development process.

## Security Features

### 1. Dependency Review (PR Checks)

**Workflow:** `.github/workflows/dependency-review.yml`

- Runs on every pull request
- Scans for known vulnerabilities in dependencies
- Fails PRs with dependencies containing moderate or higher severity vulnerabilities
- Posts summary comments on pull requests

**Configuration:**
- Severity threshold: `moderate`
- Comments are always posted to PRs

### 2. CodeQL Security Analysis

**Workflow:** `.github/workflows/codeql-analysis.yml`

- Runs on push to main branch
- Runs on pull requests
- Runs weekly on Monday at 6:00 UTC (scheduled scan)
- Analyzes C# code for security vulnerabilities and code quality issues

**Features:**
- Uses `security-extended` and `security-and-quality` query suites
- Detects common security vulnerabilities:
  - SQL injection
  - Cross-site scripting (XSS)
  - Path traversal
  - Command injection
  - Hard-coded credentials
  - And many more

**Results:**
- Available in the Security tab of the GitHub repository
- Alerts are created for identified issues

### 3. Dependabot

**Configuration:** `.github/dependabot.yml`

Automated dependency updates for:

#### NuGet Packages
- Checks weekly (Monday at 6:00 UTC)
- Opens up to 10 pull requests
- Labels: `dependencies`, `nuget`
- Commit prefix: `deps`

#### GitHub Actions
- Checks weekly (Monday at 6:00 UTC)
- Opens up to 5 pull requests
- Labels: `dependencies`, `github-actions`
- Commit prefix: `ci`

**Benefits:**
- Automatic updates for dependencies
- Security vulnerability alerts
- Keeps dependencies up to date

## Viewing Security Alerts

### Dependency Vulnerabilities
1. Navigate to the **Security** tab in the GitHub repository
2. Click on **Dependabot alerts** to see vulnerable dependencies
3. Review suggested fixes and update dependencies

### Code Scanning Alerts
1. Navigate to the **Security** tab in the GitHub repository
2. Click on **Code scanning** to see CodeQL findings
3. Review each alert and implement recommended fixes

## Best Practices

1. **Review Dependabot PRs promptly** - Keep dependencies updated to minimize security risks
2. **Address CodeQL alerts** - Fix security issues identified by static analysis
3. **Monitor the Security tab** - Regularly check for new alerts
4. **Don't bypass security checks** - All PRs must pass dependency review before merging

## Workflow Permissions

All security workflows use minimal required permissions following the principle of least privilege:

- **Dependency Review**: `contents: read`, `pull-requests: write`
- **CodeQL Analysis**: `contents: read`, `security-events: write`, `actions: read`
- **Build and Test**: `contents: read`

## Maintenance

- Security workflows are updated automatically via Dependabot
- CodeQL queries are updated automatically by GitHub
- Review and update security configurations quarterly
