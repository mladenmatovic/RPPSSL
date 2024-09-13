FROM node:22 AS build

# Check if Yarn is installed, if not, install it
RUN yarn --version || npm install -g yarn

WORKDIR /app

COPY client/package.json client/yarn.lock* ./

# Install dependencies
RUN yarn install --frozen-lockfile

# Copy the rest of your app's source code
COPY client/ .

RUN yarn build

# Production stage
FROM nginx:alpine

# Copy built assets from 'build' stage
COPY --from=build /app/build /usr/share/nginx/html

COPY client/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 3000

CMD ["nginx", "-g", "daemon off;"]