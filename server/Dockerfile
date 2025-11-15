FROM node:22-alpine AS base
WORKDIR /app

COPY package*.json ./
RUN npm install --omit=dev

COPY tsconfig.json tsconfig.json
COPY src ./src

RUN npm run build:ts

FROM node:22-alpine AS runner
WORKDIR /app
ENV NODE_ENV=production

COPY --from=base /app/package*.json ./
COPY --from=base /app/node_modules ./node_modules
COPY --from=base /app/dist ./dist

EXPOSE 3333

CMD ["node", "dist/app.js"]

