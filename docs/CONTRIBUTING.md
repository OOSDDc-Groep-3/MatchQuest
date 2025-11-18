
# Contributing to This Project

Thank you for your interest in contributing! We welcome contributions including bug fixes, new features, and documentation improvements.

## Table of Contents

* [Code of Conduct](#code-of-conduct)
* [How to Ask Questions](#how-to-ask-questions)
* [Branching and Workflow](#branching-and-workflow)
* [Making Contributions](#making-contributions)
* [Pull Requests](#pull-requests)
* [Commit Guidelines](#commit-guidelines)
* [Reporting Issues](#reporting-issues)

## Code of Conduct

This project follows a [Code of Conduct](CODE_OF_CONDUCT.md). By contributing, you agree to uphold a respectful and welcoming environment.

## How to Ask Questions

Before asking a question, please check:

* [Documentation](docs/)
* Existing [Issues](issues/)

If your question is not covered, open a new issue with details and context.

## Branching and Workflow

We follow the **GitHub flow workflow** with the following branches:

* `main`: Stable production code. Only releases are merged here.
* `develop`: Integration branch for new features and fixes.
* `feature/<feature-name>`: Branch from `develop` for new features.
* `hotfix/<hotfix-name>`: Branch from `main` for urgent fixes.

**Workflow Overview:**

1. Create a feature branch from `develop`.
2. Work on your feature and commit regularly.
3. Merge completed features back into `develop`.
4. When ready for a release, merge `develop` into `main`.

## Making Contributions

1. Start a feature or bugfix branch:

```bash
git checkout develop
git pull
git flow feature start my-feature
```

2. Commit functional changes regularly (each commit should be a working piece of code).

3. Push your branch to the remote repository:

```bash
git push origin feature/my-feature
```

## Pull Requests

* Create a pull request for every feature or fix, targeting the `develop` branch.
* If there is more than one contributor, at least one other contributor should review the PR before merging.
* Include in your PR:

  * Description of changes
  * Linked issues (if applicable)
  * Any testing performed

## Commit Guidelines

Use clear, simple, and professional commit messages. A recommended format for beginners which we use is:

```
<type>: <short description>
```

Where `<type>` can be:

* `feat`: New feature
* `fix`: Bug fix
* `docs`: Documentation update
* `style`: Formatting, spacing, or style fixes
* `refactor`: Code restructuring without changing behavior
* `test`: Adding or updating tests

**Examples:**

```
feat: add login functionality
fix: correct API request error handling
docs: update README with installation instructions
```

Commit each time you add a working piece of code that runs without errors.

## Reporting Issues

When reporting bugs or requesting enhancements:

* Check existing issues first.
* Provide clear steps to reproduce the problem.
* Include any relevant environment info, logs, or screenshots.

