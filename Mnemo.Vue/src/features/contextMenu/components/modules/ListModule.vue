<script setup lang="ts">
import type { ContextMenuOption } from '../../types/ContextMenuOption'
import MenuItem from '../MenuItem.vue'

defineProps<{
  menuOptions: ContextMenuOption[]
}>()

const emit = defineEmits<{
  (e: 'invoke', option: ContextMenuOption): void
}>()
</script>

<template>
  <div class="module-list">
    <MenuItem
      v-for="opt in menuOptions"
      :key="opt.label"
      :icon="opt.icon"
      :label="opt.label"
      :selected="opt.selected"
      :disabled="opt.disabled"
      @click="emit('invoke', opt)"
    />
  </div>
</template>

<style lang="scss" scoped>
.module-list {
  display: flex;
  flex-direction: column;
  gap: 2px;
}
</style>
