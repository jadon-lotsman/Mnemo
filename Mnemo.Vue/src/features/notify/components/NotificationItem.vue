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
  position: relative;

  display: flex;
  align-items: start;

  pointer-events: all;

  box-shadow: 5px 5px 0px $shadow;
  border-radius: 0px 12px 12px 12px;

  margin-left: 32px;
  margin-right: 20px;

  max-width: $layoutWidth;
  width: fit-content;
  max-width: 400px;
  min-width: 250px;
  padding: 10px 15px;
  padding-right: 30px;

  background-color: $clear-white;

  &::after {
    content: '';

    position: absolute;

    width: 8px;
    height: 8px;

    top: 0px;
    left: -22px;

    border-radius: 50%;

    background-color: inherit;
  }

  &::before {
    content: '';

    position: absolute;

    width: 18px;
    height: 18px;

    top: 0px;
    left: -9px;

    border-radius: 50%;

    background-color: inherit;
  }

  .icon {
    @include iconize-text;

    display: block;

    color: $plane-gray;

    margin-right: 12px;
    margin-top: 5px;

    font-size: 32px;
  }

  .title {
    display: block;

    color: $black-font;
  }

  .description {
    display: block;

    color: $gray-font;

    margin-top: 3px;

    font-size: 15px;
  }
}
</style>
