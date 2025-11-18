# Contributing
Contributing to This Project
Thank you for your interest in contributing! We welcome contributions including bug fixes, new features, and documentation improvements.

Table of Contents
Code of Conduct
How to Ask Questions
Branching and Workflow
Making Contributions
Pull Requests
Commit Guidelines
Reporting Issues
Code of Conduct
This project follows a Code of Conduct. By contributing, you agree to uphold a respectful and welcoming environment.

How to Ask Questions
Before asking a question, please check:

Documentation
Existing Issues
If your question is not covered, open a new issue with details and context.

Branching and Workflow
We follow the GitHub flow workflow with the following branches:

main: Stable production code. Only releases are merged here.
develop: Integration branch for new features and fixes.
feature/<feature-name>: Branch from develop for new features.
hotfix/<hotfix-name>: Branch from main for urgent fixes.
Workflow Overview:

Create a feature branch from develop.
Work on your feature and commit regularly.
Merge completed features back into develop.
When ready for a release, merge develop into main.
Making Contributions
Start a feature or bugfix branch:
git checkout develop
git pull
git flow feature start my-feature
Commit functional changes regularly (each commit should be a working piece of code).

Push your branch to the remote repository:

git push origin feature/my-feature
Pull Requests
Create a pull request for every feature or fix, targeting the develop branch.
If there is more than one contributor, at least one other contributor should review the PR before merging.
Include in your PR:

Description of changes
Linked issues (if applicable)
Any testing performed
Commit Guidelines
Use clear, simple, and professional commit messages. A recommended format for beginners which we use is:

<type>: <short description>
Where <type> can be:

feat: New feature
fix: Bug fix
docs: Documentation update
style: Formatting, spacing, or style fixes
refactor: Code restructuring without changing behavior
test: Adding or updating tests
Examples:

feat: add login functionality
fix: correct API request error handling
docs: update README with installation instructions
Commit each time you add a working piece of code that runs without errors.

Reporting Issues
When reporting bugs or requesting enhancements:

Check existing issues first.
Provide clear steps to reproduce the problem.
Include any relevant environment info, logs, or screenshots.
