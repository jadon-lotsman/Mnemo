<script setup lang="ts">
import NotificationItem from './NotificationItem.vue'
import { useNotificationStore } from '@/features/notify/stores/NotificationStore'

const store = useNotificationStore()
</script>

<template>
  <div class="notification-container">
    <TransitionGroup name="notification" tag="div" class="notifications-list">
      <NotificationItem
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
  bottom: 20px;

  pointer-events: none;

  width: 100%;

  z-index: 9998;
}

.notifications-list {
  display: flex;
  justify-content: end;
  flex-direction: column;

  gap: 10px;

  max-width: $layoutWidth;
  margin: 0px auto;
}

.notification-enter-active,
.notification-leave-active {
  transition: all 0.18s ease;
}
.notification-enter-from {
  opacity: 0%;
  transform: scale(0.97);
}
.notification-leave-to {
  opacity: 0%;
  transform: scale(0.97);
}
.notification-move {
  transition: transform 0.3s cubic-bezier(0.3, 0.9, 0.2, 1.1);
}
</style>
