<script setup lang="ts">
import type { ContextMenuModule } from '../types/MenuConfig'
import type { ContextMenuOption } from '../types/ContextMenuOption'
import { useContextMenu } from '@/shared/composables/useContextMenu'
import ListModule from './modules/ListModule.vue'
import PagedListModule from './modules/PagedListModule.vue'
import TextModule from './modules/TextModule.vue'

defineProps<{
  module: ContextMenuModule
}>()

const { closeMenu } = useContextMenu()

function invokeOption(option: ContextMenuOption) {
  if (option.disabled) return

  option.action()
  closeMenu()
}
</script>

<template>
  <PagedListModule
    v-if="module.type === 'search'"
    :placeholder="module.placeholder"
    :menu-options="module.options"
    :list-size="module.listSize"
    @invoke="invokeOption"
  />

  <ListModule
    v-else-if="module.type === 'list'"
    :menu-options="module.options"
    @invoke="invokeOption"
  />

  <TextModule v-else-if="module.type === 'text'" :text="module.text" @invoke="invokeOption" />
</template>

<style lang="scss" scoped></style>
