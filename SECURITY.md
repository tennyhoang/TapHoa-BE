# Security Policy

## Supported Versions

| Version | Supported |
|---------|-----------|
| latest  | ✅ Yes    |
| < 1.0   | ❌ No     |

## Reporting a Vulnerability

**Please do NOT open a public GitHub issue for security vulnerabilities.**

Report vulnerabilities privately via email:

**security@taphoa.vn**

Include in your report:
- Description of the vulnerability
- Steps to reproduce
- Potential impact
- Suggested fix (optional)

We will acknowledge your report within **48 hours** and aim to release a fix within **14 days** for critical issues.

## Scope

In scope:
- Authentication bypass or token forgery
- SQL injection, XSS, CSRF
- Unauthorized data access or privilege escalation
- Hardcoded secrets or credentials in production builds
- Exposed PII without authorization

Out of scope:
- Denial-of-service attacks
- Issues in third-party dependencies (report to the upstream project)
- Issues only reproducible in development/local environments

## Responsible Disclosure

We follow [responsible disclosure](https://en.wikipedia.org/wiki/Responsible_disclosure). We will:
- Acknowledge your report promptly
- Keep you updated on progress
- Credit you in the release notes (unless you prefer anonymity)
- Not pursue legal action for good-faith security research
