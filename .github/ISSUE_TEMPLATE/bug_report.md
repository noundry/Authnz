---
name: Bug report
about: Create a report to help us improve
title: '[BUG] '
labels: bug
assignees: ''
---

## 🐛 Bug Description

A clear and concise description of what the bug is.

## 🔄 Steps to Reproduce

1. Go to '...'
2. Click on '...'
3. Configure '...'
4. See error

## ✅ Expected Behavior

A clear and concise description of what you expected to happen.

## ❌ Actual Behavior

A clear and concise description of what actually happened.

## 🖥️ Environment

- **OS**: [e.g. Windows 11, Ubuntu 22.04]
- **.NET Version**: [e.g. .NET 8.0]
- **Noundry.Authnz Version**: [e.g. 1.1.0]
- **OAuth Provider**: [e.g. Google, GitHub]

## ⚙️ Configuration

```json
{
  "OAuth": {
    // Your OAuth configuration (remove secrets!)
  }
}
```

## 📋 Additional Context

Add any other context about the problem here, including:
- Error messages or stack traces
- Log output (with sensitive info removed)
- Screenshots if applicable

## 🔍 Troubleshooting Attempted

- [ ] Checked redirect URIs match exactly
- [ ] Verified client credentials are correct
- [ ] Tested with different OAuth providers
- [ ] Reviewed application logs
- [ ] Checked middleware order in Program.cs