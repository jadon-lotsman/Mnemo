<script setup lang="ts">
import type { ContextMenuOption } from '../../types/ContextMenuOption.ts'
import { ref, watch } from 'vue'
import MenuItem from '../MenuItem.vue'
import CustomScrollbar from '@/shared/components/CustomScrollbar.vue'

const props = withDefaults(
  defineProps<{
    placeholder: string
    menuOptions: ContextMenuOption[]
    disabled?: boolean
    listSize?: number
  }>(),
  {
    listSize: 4,
  },
)

const emit = defineEmits<{
  (e: 'invoke', option: ContextMenuOption): void
}>()

const searchRef = ref<HTMLInputElement | null>(null)
const searchQuery = ref<string>('')

watch(
  () => props.menuOptions,
  () => {
    searchQuery.value = ''
  },
)
</script>

<template>
  <div class="paged-list">
    <form class="search-form">
      <span class="icon">search</span>
      <input ref="searchRef" type="search" :disabled="disabled" :placeholder="placeholder" />
    </form>
    <div
      class="options-container"
      :style="{ height: `${Math.min(listSize, menuOptions.length) * 27.33}px` }"
    >
      <CustomScrollbar>
        <MenuItem
          v-for="opt in menuOptions"
          :key="opt.label"
          :icon="opt.icon"
          :label="opt.label"
          :selected="opt.selected"
          :disabled="opt.disabled"
          @click="emit('invoke', opt)"
        />
      </CustomScrollbar>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.paged-list {
  .search-form {
    position: relative;

    .icon {
      @include iconize(16px);

      position: absolute;

      top: 8px;
      left: 12px;
    }

    input {
      border-radius: 8px;
      background-color: $surface-primary;
      padding: 7px 10px;
      padding-left: 35px;

      width: 100%;

      color: $text-muted;
      font-size: 15px;
    }
  }

  .options-container {
    margin: 4px 20px 4px 8px;
  }
}
</style>
