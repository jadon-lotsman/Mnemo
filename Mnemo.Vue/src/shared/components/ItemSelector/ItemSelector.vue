<script setup lang="ts">
import { ref } from 'vue'
import type { SelectorItem } from './SelectorItem'
import { useContextMenu } from '@/shared/composables/useContextMenu'
import type { ContextMenuOption } from '@/features/contextMenu/types/ContextMenuOption'

const props = defineProps<{
  icon: string
  items: SelectorItem[]
}>()

const { openContextByElement } = useContextMenu()

const selected = ref<SelectorItem | null>(props.items[0] ?? null)
const selector = ref<HTMLElement | null>(null)

const emit = defineEmits<{
  (e: 'itemChanged', item: SelectorItem): void
}>()

function selectItem(item: SelectorItem) {
  if (selected.value == item) return

  selected.value = item
  emit('itemChanged', item)
}

function openList() {
  const contextMenuItems: ContextMenuOption[] = props.items.map((item) => ({
    label: item.label,
    icon: item.icon,
    action: () => selectItem(item),
  }))

  contextMenuItems.push()

  openContextByElement(selector.value, contextMenuItems)
}
</script>

<template>
  <div ref="selector" class="selector-container" @click="openList">
    <span class="icon">{{ icon }}</span>
    <div class="current-item">
      <span class="label">{{ selected?.label || 'None' }}</span>
      <span class="chevron">chevron_forward</span>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.selector-container {
  display: flex;
  align-items: center;

  gap: 5px;

  cursor: pointer;

  margin-bottom: 6px;
  padding-left: 4px;

  color: $text-secondary;

  .icon {
    font-size: 18px;
    @include iconize;
  }

  .current-item {
    display: flex;
    position: relative;
    align-items: center;

    .label {
      padding-right: 24px;
    }

    .chevron {
      @include iconize;

      position: absolute;

      top: 0px;
      right: 0px;

      transform: rotate(90deg);
      margin-left: -3px;

      font-size: 21px;
    }
  }
}
</style>
