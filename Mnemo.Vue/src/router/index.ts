import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import AppView from '../views/AppView.vue'

export const ROUTE_PATHS = {
  LOGIN: '/login',
  APP: '/app',
}

export const ROUTE_NAMES = {
  LOGIN: 'login',
  APP: 'app',
}

const routes = [
  {
    path: ROUTE_PATHS.LOGIN,
    name: ROUTE_NAMES.LOGIN,
    component: LoginView,
  },
  {
    path: ROUTE_PATHS.APP,
    name: ROUTE_NAMES.APP,
    component: AppView,
    meta: { requiresAuth: true },
  },
  {
    path: '/',
    redirect: { name: ROUTE_NAMES.APP },
  },
]

function hasValidToken(): boolean {
  const token = localStorage.getItem('token')
  if (!token) return false

  try {
    const payload = JSON.parse(atob(token.split('.')[1] ?? ''))
    const exp = payload.exp
    if (exp && Date.now() >= exp * 1000) {
      localStorage.removeItem('token')
      return false
    }
  } catch {
    return false
  }
  return true
}

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

router.beforeEach((to, from, next) => {
  const authenticated = hasValidToken()

  if (to.meta.requiresAuth && !authenticated) {
    next({ name: ROUTE_NAMES.LOGIN, query: { sessionExpired: 'true' } })
  } else if (to.name === ROUTE_NAMES.LOGIN && authenticated) {
    next({ name: ROUTE_NAMES.APP })
  } else {
    next()
  }
})

export default router
