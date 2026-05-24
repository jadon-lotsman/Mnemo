<script setup lang="ts">
import NotificationItemComponent from '@/features/notify/components/NotificationItemComponent.vue'
import { useNotificationStore } from '@/features/notify/stores/NotificationStore'

const store = useNotificationStore()
</script>

<template>
  <div class="notification-container">
    <NotificationItemComponent
      v-for="notification in store.notifications"
      :key="notification.id"
      :data="notification"
    />
  </div>
</template>

<style lang="scss" scoped>
.notification-container {
  position: fixed;
  left: 0;

  display: flex;
  justify-content: end;
  flex-direction: column;
  gap: 5px;

  width: 100%;
  bottom: 20px;
}
</style>
