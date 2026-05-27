import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { Notification } from '../types/Notification'

export const useNotificationStore = defineStore('notification', () => {
  const notifications = ref<Notification[]>([])

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
