import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import VocabularyView from '@/views/VocabularyView.vue'
import SessionView from '@/views/SessionView.vue'

export const ROUTE_PATHS = {
  LOGIN: '/login',
  VOCABULARY: '/vocabulary',
  SESSION: '/session',
}

export const ROUTE_NAMES = {
  LOGIN: 'login',
  VOCABULARY: 'vocabulary',
  SESSION: 'session',
}

const routes = [
  {
    path: ROUTE_PATHS.LOGIN,
    name: ROUTE_NAMES.LOGIN,
    component: LoginView,
  },
  {
    path: ROUTE_PATHS.VOCABULARY,
    name: ROUTE_NAMES.VOCABULARY,
    component: VocabularyView,
    meta: { requiresAuth: true },
  },
  {
    path: ROUTE_PATHS.SESSION,
    name: ROUTE_NAMES.SESSION,
    component: SessionView,
    meta: { requiresAuth: true },
  },
  {
    path: '/',
    redirect: { name: ROUTE_NAMES.VOCABULARY },
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
    next({ name: ROUTE_NAMES.VOCABULARY })
  } else {
    next()
  }
})

export default router
