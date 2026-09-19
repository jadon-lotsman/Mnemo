<script setup lang="ts" generic="T">
import { ref, watch } from 'vue'
import type { SelectorItem } from './SelectorItem'
import { useContextMenu } from '@/shared/composables/useContextMenu'
import type { ContextMenuOption } from '@/features/contextMenu/types/ContextMenuOption'

const props = defineProps<{
  modelValue: SelectorItem<T> | null
  icon: string
  items: SelectorItem<T>[]
}>()

const { openContextByElement } = useContextMenu()

const selector = ref<HTMLElement | null>(null)

const emits = defineEmits<{
  (e: 'update:modelValue', value: SelectorItem<T> | null): void
}>()

function selectItem(item: SelectorItem<T> | null) {
  if (props.modelValue === item) return
  emits('update:modelValue', item)
}

function openList() {
  const contextMenuItems: ContextMenuOption[] = props.items.map((item) => ({
    label: item.label,
    icon: item.icon,
    action: () => selectItem(item),
  }))

  openContextByElement(selector.value, contextMenuItems)
}

watch(
  () => props.items,
  (newItems) => {
    if (newItems.length === 0) return
    if (props.modelValue && newItems.some((i) => i.value === props.modelValue!.value)) return
    selectItem(newItems[0] ?? null)
  },
  { immediate: true },
)
</script>

<template>
  <div ref="selector" class="selector-container" @click="openList">
    <span class="icon">{{ icon }}</span>
    <div class="current-item">
      <span class="label">{{ modelValue?.label || 'None' }}</span>
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
  padding-left: 3px;

  color: $text-secondary;

  .icon {
    font-size: 20px;
    @include iconize;
  }

  .current-item {
    display: flex;
    position: relative;
    align-items: center;

    margin-top: 1px;

    min-width: 0;

    font-size: 16px;

    .label {
      @include ellipsis;

      padding-right: 22px;

      min-width: 0;
    }

    .chevron {
      @include iconize;

      position: absolute;

      top: 0px;
      right: 0px;

      transform: rotate(90deg);
      margin-left: -3px;

      color: $text-muted;

      font-size: 22px;
    }
  }
}
</style>
