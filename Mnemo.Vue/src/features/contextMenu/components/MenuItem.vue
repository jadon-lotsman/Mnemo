<!-- MenuItem.vue -->
<script setup lang="ts">
defineProps<{
  icon?: string
  label: string
  disabled?: boolean
}>()

defineEmits<{ (e: 'click'): void }>()
</script>

<template>
  <div
    class="menu-item"
    :class="{ disabled }"
    @mousedown.prevent
    @click="!disabled && $emit('click')"
  >
    <span v-if="icon" class="icon">{{ icon }}</span>
    <span class="label">{{ label }}</span>
  </div>
</template>

<style lang="scss" scoped>
.menu-item {
  display: flex;
  align-items: center;
  cursor: pointer;

  border-radius: 8px;

  padding: 4px;

  .icon {
    @include iconize;
    margin-right: 12px;
    margin-left: 8px;
    color: $shadow-color;
    font-size: 21px;
    line-height: 0.8;
  }

  &:hover {
    background-color: $surface-secondary;
  }

  &.disabled {
    cursor: default;
    color: $shadow-color;
    .icon {
      opacity: 65%;
    }
  }
}
</style>
