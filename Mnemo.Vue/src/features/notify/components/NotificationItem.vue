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
  background-color: $cloud-white;

  filter: drop-shadow(0px 0px 8px #bbbbbb4d);
  backdrop-filter: blur(2px);

  margin-left: 15px;
  margin-right: 12px;

  max-width: $layoutWidth;
  width: fit-content;
  max-width: 400px;
  min-width: 250px;
  padding: 10px 15px;
  padding-right: 25px;

  &::after {
    content: '';

    position: absolute;

    width: 0;
    height: 0;
    border: 8px solid transparent;
    border-top: 8px solid $cloud-white;
    border-right: 8px solid $cloud-white;

    top: 0px;
    left: -12px;

    background-color: transparent;
  }

  .icon {
    @include iconize-text;

    display: block;

    opacity: 85%;

    color: $shadow;

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
