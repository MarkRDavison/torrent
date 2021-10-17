FROM node:alpine as BUILD
WORKDIR /app

COPY / /app/
RUN npm install
RUN npm run build

ENV PATH /usr/src/app/node_modules/.bin:$PATH

# remove development dependencies
RUN npm prune --production

FROM node:alpine
WORKDIR /app
COPY --from=BUILD /app/dist .
COPY --from=BUILD /app/node_modules ./node_modules

ENTRYPOINT [ "node", "index.js" ]