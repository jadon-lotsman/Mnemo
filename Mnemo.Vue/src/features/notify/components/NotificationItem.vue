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
  position: relative;
  align-items: start;

  -webkit-backdrop-filter: blur(2px);
  backdrop-filter: blur(2px);

  will-change: transform, opacity;

  margin-right: 12px;
  margin-left: 15px;

  box-shadow: 5px 5px 0px $shadow-color;
  border-radius: 0px 12px 12px 12px;

  background-color: $elevated-bg;

  padding: 10px 15px;
  padding-right: 25px;

  width: fit-content;
  min-width: 250px;
  max-width: $layout-width;

  pointer-events: all;

  &::after {
    position: absolute;

    top: 0px;
    left: -11px;

    border: 6px solid transparent;
    border-top: 6px solid $elevated-bg;
    border-right: 6px solid $elevated-bg;

    background-color: transparent;

    width: 0;
    height: 0;

    content: '';
  }

  .icon {
    @include iconize(25px);

    display: block;

    opacity: 80%;

    margin-top: 2px;
    margin-right: 17px;

    color: $icon-color;
  }

  .title {
    display: block;

    color: $text-primary;
  }

  .description {
    display: block;

    margin-top: 3px;

    color: $text-secondary;

    font-size: 15px;
  }
}
</style>
