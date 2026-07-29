---
applyTo: "{src/UI/WB.UI.Frontend/src/**,src/UI/WB.UI.Designer/questionnaire/src/**/*}.{js,ts,vue,jsx,tsx}"
---

# Frontend Conventions — Survey Solutions

There are **two distinct frontend applications** with different technology choices.

---

## 1. `WB.UI.Frontend` — Headquarters SPA + WebInterview

**Location:** `src/UI/WB.UI.Frontend/`

**Build output destination:** `src/UI/WB.UI.Headquarters.Core/` and `src/UI/WB.UI.WebTester/` (Vite injects asset hashes directly into `.cshtml` layout templates).

### Entry Points

| Entry | Target |
|---|---|
| `src/hqapp/main.js` | Headquarters SPA (`_Layout.cshtml`) |
| `src/webinterview/main.js` | WebInterview SPA (`WebInterview/Index.cshtml`) |
| `src/pages/*.js` | Standalone pages (login, finish-install, etc.) |

### Tech Stack

- **Vue 3** (Composition API supported; Options API used in legacy components)
- **Vuex 4** — state management for HQ app and WebInterview (`createStore`, modules)
- **Vue Router 4**
- **Axios** — HTTP calls to backend API
- **Apollo / GraphQL** (`@apollo/client`, `@vue/apollo-option`) — used in some HQ views
- **SignalR** (`@microsoft/signalr`) — real-time WebInterview communication
- **ag-Grid Community** — data grids
- **Bootstrap 5** + **jQuery** (legacy; avoid adding new jQuery code)
- **i18next** — localization (translations loaded from server-generated resource files)
- **DOMPurify** (`vue-dompurify-html`) — sanitize HTML in user-generated content
- **Vite** — build tool

### State Management (Vuex)

- Organize store as Vuex modules; see `src/webinterview/stores/` for split-file module pattern (`actions`, `mutations`, `state`, `getters` in separate files).
- **Do not** introduce Pinia in this project — it uses Vuex 4 only.

### Testing

- **Jest** (`npm test` in `WB.UI.Frontend`)
- Vue Test Utils (`@vue/test-utils`) for component tests
- Test files under `tests/unit/`, named `*.spec.js`

---

## 2. `WB.UI.Designer` — Questionnaire Designer SPA

**Location:** `src/UI/WB.UI.Designer/questionnaire/`

**Build output destination:** `src/UI/WB.UI.Designer/wwwroot/` (Vite outputs to the ASP.NET Core static files folder).

### Tech Stack

- **Vue 3** (Options API in stores; Composition API via `<script setup>` in newer components)
- **Pinia** (`defineStore`) — state management (**not** Vuex)
- **Vue Router 4**
- **Vuetify 3** — Material Design component library
- **Mande** — lightweight HTTP client wrapping `fetch`
- **Bootstrap 5** + **Less** for additional styling
- **i18next** + `i18next-vue` — localization
- **Ace Editor** (`vue3-ace-editor`, `ace-builds`) — expression/script editing
- **DOMPurify** (`vue-dompurify-html`) — sanitize HTML
- **Vite** — build tool

### State Management (Pinia)

Use the `defineStore` pattern with `state`, `getters`, and `actions`:

```js
export const useMyStore = defineStore('myStore', {
    state: () => ({ ... }),
    getters: { ... },
    actions: { ... },
})
```

- Pinia is the primary state management library. Vuex is still used in legacy classifications pages; **do not** introduce new Vuex modules — new state should use Pinia.
- Pinia stores live in `questionnaire/src/stores/`.

### API Layer

HTTP calls use the `commandCall`, `get`, `getSilently` helpers from `questionnaire/src/services/apiService.js` (wraps `mande`). These helpers automatically manage progress indicators and error toasts.

### Testing

No automated test suite is configured for the Designer frontend; rely on manual and integration testing.
