# =============================================================================
# Stage 1: Build
# =============================================================================
FROM node:22-alpine AS build
WORKDIR /app

# Copy package files first for layer caching — npm install only re-runs
# when package.json / package-lock.json change, not on every code edit.
COPY package.json package-lock.json ./
RUN npm ci --ignore-scripts

# Copy source and build for production
COPY . .
RUN npx ng build --configuration production

# =============================================================================
# Stage 2: Runtime (nginx serves the static files)
# =============================================================================
FROM nginx:1.27-alpine AS runtime

# Remove the default nginx static assets
RUN rm -rf /usr/share/nginx/html/*

# Copy the Angular build output
COPY --from=build /app/dist/ticketflow-web/browser /usr/share/nginx/html

# Copy our custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
