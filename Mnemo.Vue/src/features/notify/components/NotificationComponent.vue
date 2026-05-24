<script setup lang="ts">
import NotificationItemComponent from '@/features/notify/components/NotificationItemComponent.vue'
import { useNotificationStore } from '@/features/notify/stores/NotificationStore'

const store = useNotificationStore()
</script>

<template>
  <div class="notification-container">
    <TransitionGroup name="notification" tag="div" class="notifications-list">
      <NotificationItemComponent
        v-for="notification in store.notifications"
        :key="notification.id"
        :data="notification"
      />
    </TransitionGroup>
  </div>
</template>

<style lang="scss" scoped>
.notification-container {
  position: fixed;
  left: 0;

  pointer-events: none;

  width: 100%;
  bottom: 20px;
}

.notifications-list {
  display: flex;
  justify-content: end;
  flex-direction: column;
  gap: 5px;
}

.notification-enter-active,
.notification-leave-active {
  transition: all 0.3s ease;
}
.notification-enter-from {
  opacity: 0%;
}
.notification-leave-to {
  opacity: 0%;
  transform: scale(0.98);
}
.notification-move {
  transition: transform 0.3s cubic-bezier(0.2, 0.9, 0.4, 1.1);
}
</style>
