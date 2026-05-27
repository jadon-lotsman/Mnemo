import router from '@/router'
import { ROUTE_NAMES } from '@/router'
import { useNotify } from '../composables/useNotify'

export async function apiRequest<T = unknown>(url: string, options: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem('token')
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(token && { Authorization: `Bearer ${token}` }),
    ...options.headers, // пользовательские заголовки (могут переопределить)
  }

  const notify = useNotify()

  try {
    const response = await fetch(url, { ...options, headers })

    // Удаляем недействительный токен и переадресуем
    if (response.status === 401) {
      localStorage.removeItem('token')

      if (router.currentRoute.value.path !== ROUTE_NAMES.LOGIN) {
        router.push({ name: ROUTE_NAMES.LOGIN, query: { sessionExpired: 'true' } })
      }

      throw new Error('Hmm...')
    }

    if (!response.ok) {
      throw new Error(`${response.status}: ${response.statusText}`)
    }

    // Возвращаем удачный ответ
    return (await response.json()) as T
  } catch (err: unknown) {
    const error = err as Error
    const errorMessage = error.message ?? 'Network error...'
    notify.failure(errorMessage)
    throw error
  }
}
