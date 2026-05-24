import { useNotificationStore } from '@/features/notify/stores/NotificationStore'

export function useNotify() {
  const store = useNotificationStore()

  function success(message: string) {
    return store.addNotification('success', message)
  }

  function failure(message: string) {
    return store.addNotification('failure', message)
  }

  function info(message: string) {
    return store.addNotification('info', message)
  }

  return { success, failure, info }
}
