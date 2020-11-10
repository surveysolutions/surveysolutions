ARG NODEJS_VERSION=14
ARG DOTNET_VERSION=3.1
ARG GIT_BRANCH=master

FROM --platform=linux/amd64 node:${NODEJS_VERSION} AS questionnaire_app
COPY src/UI/WB.UI.Designer/questionnaire-app/package*.json /src/UI/WB.UI.Designer/questionnaire-app/

RUN cd /src/UI/WB.UI.Designer/questionnaire-app && npm ci
COPY /src/UI/WB.UI.Designer /src/UI/WB.UI.Designer

RUN cd /src/UI/WB.UI.Designer/questionnaire-app && npm run build    

FROM --platform=linux/amd64 node:${NODEJS_VERSION} AS js_builder
COPY src/UI/WB.UI.Frontend/package*.json src/UI/WB.UI.Frontend/
RUN cd src/UI/WB.UI.Frontend && npm ci

COPY src/UI/WB.UI.Frontend src/UI/WB.UI.Frontend
COPY src/UI/WB.UI.Headquarters.Core/Resources src/UI/WB.UI.Headquarters.Core/Resources
COPY src/UI/WB.UI.Headquarters.Core/Views src/UI/WB.UI.Headquarters.Core/Views
COPY src/Core/SharedKernels/Enumerator/WB.Enumerator.Native/Resources src/Core/SharedKernels/Enumerator/WB.Enumerator.Native/Resources
COPY src/Core/BoundedContexts/Headquarters/WB.Core.BoundedContexts.Headquarters/Resources src/Core/BoundedContexts/Headquarters/WB.Core.BoundedContexts.Headquarters/Resources

WORKDIR /src/UI/WB.UI.Frontend
RUN  npm run build -- --modern --package

FROM scratch
COPY --from=js_builder /src/UI/WB.UI.Frontend/dist/package /package
COPY --from=questionnaire_app /src/UI/WB.UI.Designer/questionnaire-app/dist /questionnaire_app

LABEL org.opencontainers.image.title="Headquarters JS ui"
LABEL org.opencontainers.image.description="Survey Solutions Headquarters"

