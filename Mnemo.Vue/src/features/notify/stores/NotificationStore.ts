import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { NotificationItem } from '../types/NotificationItem'

export const useNotificationStore = defineStore('notification', () => {
  const notifications = ref<NotificationItem[]>([])

  function addNotification(type: string, message: string) {
    if (notifications.value.length >= 5) return

    const id = Date.now() + Math.random()

    notifications.value.push({
      id: id,
      type: type,
      message: message,
    })

    setTimeout(() => {
      removeNotification(id)
    }, 3000)
  }

  function removeNotification(id: number) {
    notifications.value = notifications.value.filter((n) => n.id !== id)
  }

  return {
    notifications,
    addNotification,
    removeNotification,
  }
})
