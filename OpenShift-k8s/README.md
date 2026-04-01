# OpenShift Kubernetes project setup

Terraform layouts, Kustomize manifests (dev / staging / prod), GitHub Actions CI, optional beer-app sample image, Tekton/GitLab/ArgoCD samples.

## Layout

- `terraform/` — environment roots (`dev`, `staging`, `prod`) and `modules/platform`
- `k8s/` — Kustomize base, overlays, OpenShift Route component
- `beer-app/` — small nginx static image (companion to other repos; not the VST)
- `scripts/` — local validation and helper scripts
- `pipelines/` — GitLab CI example, Tekton pipeline
- `gitops/argocd/` — Argo CD `Application` stub

## CI

On push to `main`, workflows run Terraform validate and `kubectl kustomize` for all overlays. Changing `beer-app/**` builds and pushes to GHCR when not a PR.

## Local checks

```powershell
.\scripts\validate-local.ps1
```

## License

Add your license as needed.
