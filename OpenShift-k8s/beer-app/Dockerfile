# Companion web UI for the beer-can project (the JUCE VST does not run in Kubernetes).
FROM nginxinc/nginx-unprivileged:1.27-alpine

COPY static/ /usr/share/nginx/html/

EXPOSE 8080
