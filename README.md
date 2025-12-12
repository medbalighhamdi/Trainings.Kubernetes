# WellArchitectedLab.MonoRepo

> **TLDR**
>
> This mono-repo is a full, working microservices cloud platform reference: it includes application source code, Infrastructure-as-Code to provision cloud resources (AKS, ACR, networking, RBAC), Kubernetes manifests and overlays, local development scaffolding, plus CI/CD pipelines and developer tooling. The goal is to show a realistic end-to-end developer + DevOps experience: from local dev to CI, tests, container images, cluster deployment and GitOps-based delivery (Argo CD).

---

## Table of contents

- Introduction
- Repository layout (one major section per root folder)
  - `.github/` (workflows & PR templates)
  - `iac/` (Infrastructure as Code)
  - `k8s/` (Kubernetes manifests & AKS setup)
  - `src/` (application source, tests, docker-compose)
  - `setup/` (aliases & helper scripts)
- Running the solution locally
- Developer & DevOps notes
- Contributing
- License

---

# Introduction

This mono-repo demonstrates a production-minded, full-stack microservices reference platform. It was assembled to teach and prove the complete delivery lifecycle: local development, automated testing, containerization, infrastructure provisioning (IaC), Kubernetes cluster management (AKS), CI using GitHub Actions, and continuous delivery using GitOps (Argo CD). The repo is intentionally opinionated: it shows how the pieces connect so you can reuse patterns directly in real projects or list it as a demonstrated project on your CV.

Key intentions:
- Keep a single repository containing all artifacts needed to build, test, package and deploy the example microservices.
- Provide IaC templates so the platform can be provisioned in Azure (AKS, ACR, networking, RGs, etc.).
- Illustrate GitHub Actions-based CI and GitOps with Argo CD for delivery.
- Make it easy for a developer to run everything locally using the provided `docker-compose` (under `src/`).

---

# Repository layout

## `.github/`

Contains repository-level GitHub configuration. Two important sub-areas to call out:

### `.github/workflows/`

All GitHub Actions workflows live here. They automate builds, tests, releases and other repo-level automation. See the folder for the exact YAML workflows. Examples of responsibilities these workflows typically cover:

- **Build & Test**: compile projects, run unit tests and publish test results.
- **Static Analysis**: run static checks for code quality using CodeCov.
- **Publish Artifacts**: build container images and push to the configured container registry (ACR) or publish packages.
- **Release & Tagging**: create Git tags/releases and optionally trigger GitOps pipelines.
- **CI Helpers**: maintenance tasks such as branch housekeeping or dependency updates.

(You can review the actual workflow files here.)

> Workflow folder: `/.github/workflows/` — **see**: https://github.com/WellArchitectedLabs/WellArchitectedLab.MonoRepo/tree/develop/.github/workflows. 

### `.github/PULL_REQUEST_TEMPLATE/`

Contains PR templates used to standardize contribution details, checklist and required information for reviewers. This helps maintain quality and ensures important information (testing, migration steps, security considerations) is included in each PR.
The Pull Request Templates are used by git pr alias that you can install via installation script placed under `scripts/git` folder — **see**: https://github.com/WellArchitectedLabs/WellArchitectedLab.MonoRepo/tree/develop/scripts/git/aliases. 

---

## `iac/` — Infrastructure as Code

**Purpose:** this folder holds the IaC templates and automation used to provision the Azure resources required by the platform (AKS cluster, Azure Container Registry, networking, RBAC roles, and other platform-level resources).

**What to expect in this folder:**
- The root README and repository overview mention `iac/` holds Terraform / ARM templates for AKS provisioning, network, security and RBAC setup, plus Azure resource definitions. These templates are the source of truth for creating the CI/CD environment and the ACR used by the cluster. 
- The repo`s approach favors automated provisioning so the cluster and registry can be created from source-controlled files, enabling reproducible lab environments.

**How to use:**
1. Install prerequisites (terraform / az cli / azure credentials).
2. Change directory into `iac/` and follow any environment-specific variables or `README` inside `iac/` (if present).
3. Run the standard Terraform flow (`terraform init`, `terraform plan`, `terraform apply`) or the equivalent ARM/az CLI flows described in the folder.

**Notes about ACR & AKS attachment:**
The repository uses (for demo) a permissive ACR access approach: the README documents that ACR was left public to avoid needing premium private endpoints. The recommended production approach is to use private endpoints or managed identity + role assignments. A sample `az aks --attach-acr` command is documented in the repo to attach ACR to the AKS cluster.

---

## `k8s/` — Kubernetes manifests & AKS deployment

**Purpose:** store Kustomize overlays, Kubernetes manifests, Ingress/HTTP routes, services, deployments, and any cluster-level manifests (monitoring, logging, argocd application manifests).

**What to expect in this folder:**
- Deployments for backend and frontend components.
- Service and Ingress resources to expose apps.
- Kustomize overlays (e.g. `overlays/dev`) to wire up environment-specific values.
- A `gitops` (or similarly named) subfolder containing Argo CD `Application` manifest(s) to sync the repo with the cluster.

**AKS setup notes:**
- The README's Azure Container Registry attachment guidance is relevant here — attach ACR to AKS with `az aks update --attach-acr <acr>` to allow node pools to pull images. This instruction is placed in the repo README under the AKS notes and is reiterated in this `k8s` section because it is part of cluster setup. 
- There are also Argo CD instructions (install Argo CD via Helm, then apply the `gitops` application manifest). See the repo README for the referenced gitops application manifest name. 

---

## `src/` — application source & local dev artifacts

**Purpose:** host the microservices, frontend, API, shared libraries, tests and the `docker-compose` file to run the system locally.

**Compose file:** `src/docker-compose.yml` — this is the canonical local development composition that allows you to run the platform (or a reduced set of services) locally using Docker or Podman.

**Tech stack:**
- Backend: **ASP.NET Core** (C#)
- Unit testing: **NUnit**
- Assertions & helpers: **Shouldly**, **AutoFixture**, **Moq**

**Run locally (recommended short flow):**
1. Install Docker (or Podman).
2. From repo root: `cd src` and `docker-compose up --build` (or run specific services with `docker-compose up --build backend`).
3. Verify services start and use the logs to identify binding ports.

---

## `setup/`

Contains convenience scripts for developers and ops, e.g. PowerShell scripts to install helpful shell aliases.

**Notable file:**
- `setup/aliases/install-git-pr-alias.ps1` — a setup script that configures a `git pr` alias on Windows PowerShell (or Git Bash depending on usage). This alias is installed by the setup helper and simplifies creating and managing GitHub pull requests from the command line. Make sure developers run the install script during onboarding if they want the `git pr` convenience alias. 

---

# Running solution locally (updated)

**Canonical local start:** use the `docker-compose.yml` under `src/` (not the old `dotnet run` + `npm run` description that appears elsewhere). The compose file is the maintained local orchestration and will start the backend and frontend containers in a consistent way for development. See: `src/docker-compose.yml`: https://github.com/WellArchitectedLabs/WellArchitectedLab.MonoRepo/tree/develop/src/docker-compose.yml

---

# Developer & DevOps notes (CI, PRs, Git aliases)

- All GitHub Actions workflows are in `/.github/workflows/` — review them to understand CI jobs, test runs, and publishing behavior. 
- PR templates live in `/.github/PULL_REQUEST_TEMPLATE/` to standardize contribution information. 
- The repo ships a setup script to configure a `git pr` alias for streamlined PR operations: `setup/aliases/install-git-pr-alias.ps1`. Run that when onboarding. 

---

# Contributing

Please follow these minimal rules:
1. Use feature branches (`feature/<short-desc>`).
2. Run unit tests locally and ensure they pass.
3. Update documentation and READMEs for changes that affect usage or setup.
4. Use the PR template from `.github/PULL_REQUEST_TEMPLATE/` to create high-quality PRs.

---

# License

This project is provided for training/demonstration purposes.

---