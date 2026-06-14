<script setup lang="ts">
import { capitalize } from '@/shared/utils/StringExtension'
import type { Notification } from '../types/Notification'
import { useNotificationStore } from '../stores/NotificationStore'

const props = defineProps<{ data: Notification }>()

const store = useNotificationStore()

function close() {
  store.removeNotification(props.data.id)
}
</script>

<template>
  <div class="notification" @click="close()">
    <span v-if="data.type === 'success'" class="icon">check_circle</span>
    <span v-else-if="data.type === 'failure'" class="icon">cancel</span>
    <span v-else class="icon">info</span>

    <div>
      <span class="title">{{ capitalize(data.type) }}</span>
      <span class="description">{{
        capitalize(data.message) + (data.message.endsWith('.') ? '' : '.')
      }}</span>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.notification {
  display: flex;
  align-items: start;

  pointer-events: all;

  border-radius: 12px;

  max-width: 400px;
  padding: 10px 15px;

  opacity: 98%;

  background-color: $clear-white;

  .icon {
    @include iconize-text;

    display: block;

    color: $plane-gray;

    margin-right: 10px;
    margin-top: 5px;

    font-size: 30px;
  }

  .title {
    display: block;

    color: $black-font;
  }

  .description {
    display: block;

    color: $gray-font;

    margin-top: 2px;

    font-size: 16px;
  }
}
</style>
