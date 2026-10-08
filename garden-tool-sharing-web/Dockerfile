# ---- Build stage ----
FROM node:22-alpine AS build
WORKDIR /app

# Install dependencies first so this layer is cached until package files change
COPY package*.json ./
RUN npm ci

COPY . .

# Vite bakes this in at build time, so it must be a build argument (see docker-compose.yml)
ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

# ---- Runtime stage ----
FROM nginx:alpine AS final
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80