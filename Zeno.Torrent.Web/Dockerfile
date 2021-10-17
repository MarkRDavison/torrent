FROM node:alpine AS BUILD_IMAGE

RUN apk update

WORKDIR /usr/src/app

ENV PATH /usr/src/app/node_modules/.bin:$PATH

COPY . .
RUN npm install
RUN npm run build

# remove development dependencies
RUN npm prune --production

FROM node:alpine

WORKDIR /usr/src/app

COPY --from=BUILD_IMAGE /usr/src/app/build ./build
COPY --from=BUILD_IMAGE /usr/src/app/package.json .
COPY --from=BUILD_IMAGE /usr/src/app/server.mjs .
COPY --from=BUILD_IMAGE /usr/src/app/node_modules ./node_modules

CMD ["npm", "run", "serve:served"]