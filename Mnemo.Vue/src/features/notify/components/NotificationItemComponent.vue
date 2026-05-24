<script setup lang="ts">
import type { NotificationItem } from '../types/NotificationItem'

const props = defineProps<{ data: NotificationItem }>()

function capitalize(str: string): string {
  if (!str) return str
  return str.charAt(0).toUpperCase() + str.slice(1)
}
</script>

<template>
  <div class="notification">
    <span v-if="props.data.type === 'success'" class="icon">check_circle</span>
    <span v-else-if="props.data.type === 'failure'" class="icon">cancel</span>
    <span v-else class="icon">info</span>
    <div>
      <span class="title">{{ capitalize(props.data.type) }}</span>
      <span class="description">{{ capitalize(props.data.message) }}</span>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.notification {
  display: flex;
  align-items: center;

  pointer-events: all;

  border-radius: 8px;

  width: 400px;
  padding: 10px;
  margin: 0px auto;

  background-color: $clear-white;

  .icon {
    display: block;

    color: $plane-gray;

    margin-right: 10px;

    font-size: 30px;
    font-family: $iconizeFont;
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
