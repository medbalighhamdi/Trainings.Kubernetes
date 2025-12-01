# Trainings.Kubernetes

A comprehensive training repository demonstrating front-to-back application deployment on Azure Kubernetes Service (AKS) infrastructure.

## Overview

This repository provides practical examples and hands-on training for deploying applications to AKS, covering infrastructure provisioning, application development, and Kubernetes orchestration.

## Repository Structure

### 📁 k8s/
Contains Kubernetes manifests and deployment configurations:
- Deployment configurations
- Service definitions
- ConfigMaps and Secrets
- Ingress controllers
- Monitoring and logging setup

### 📁 iac/
Infrastructure as Code (IaC) templates:
- Terraform/ARM templates for AKS cluster provisioning
- Network configuration
- Security policies and RBAC
- Azure resource definitions

### 📁 src/
Application source code:
- Frontend applications
- Backend services
- API implementations
- Configuration files

## Getting Started

1. **Prerequisites**
    - Azure CLI installed and configured
    - kubectl configured for AKS access
    - Docker installed for container builds

2. **Infrastructure Setup**
    ```bash
    cd iac/
    # Follow infrastructure deployment instructions
    ```

3. **Application Deployment**
    ```bash
    cd k8s/
    kubectl apply -f .
    ```

## Contributing

Please follow standard Git practices and ensure all deployments are tested before submitting pull requests.

## License

This project is for training purposes only.