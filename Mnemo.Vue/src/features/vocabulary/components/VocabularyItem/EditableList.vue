<script lang="ts" setup>
import { useNotify } from '@/shared/composables/useNotify'
import { capitalize } from '@/shared/utils/StringExtension'
import { computed, ref, watch } from 'vue'

const notify = useNotify()

defineOptions({ inheritAttrs: false })

const props = withDefaults(
  defineProps<{
    placeholder: string
    existItems: string[]
    isEditorMode: boolean
    capitalizeItems?: boolean
    markItems?: boolean
  }>(),
  {
    capitalizeItems: false,
    markItems: false,
  },
)

const emit = defineEmits<{
  (e: 'update', added: string[], removed: string[]): void
}>()

const itemsAdded = ref<string[]>([])
const itemsRemoved = ref<string[]>([])
const resultItems = computed(() => {
  const all = [...props.existItems, ...itemsAdded.value]
  return all.filter((item) => !itemsRemoved.value.includes(item))
})

const inputValue = ref<string>('')

function pushItem() {
  const str = inputValue.value.toLowerCase()

  if (resultItems.value.includes(str)) {
    notify.info('Already exists')
    return
  }

  if (itemsRemoved.value.includes(str)) {
    itemsRemoved.value = itemsRemoved.value.filter((item) => item !== str)
  } else {
    itemsAdded.value.push(inputValue.value)
  }

  inputValue.value = ''
  emitChanges()
}

function removeItem(str: string) {
  str = str.toLowerCase()

  if (!resultItems.value.includes(str)) return

  if (itemsAdded.value.includes(str)) {
    itemsAdded.value = itemsAdded.value.filter((item) => item !== str)
  } else {
    itemsRemoved.value.push(str)
  }

  emitChanges()
}

function emitChanges() {
  emit('update', itemsAdded.value, itemsRemoved.value)
}

watch(
  () => props.existItems,
  () => {
    itemsAdded.value = []
    itemsRemoved.value = []
  },
)
</script>

<template>
  <ol v-if="resultItems.length > 0">
    <div
      class="editable-item"
      :class="{ marked: markItems }"
      v-for="item in resultItems"
      :key="item"
    >
      <button v-if="isEditorMode" @click.stop="removeItem(item)">close</button>
      <li>
        {{ capitalizeItems ? capitalize(item) : item }}
      </li>
    </div>
  </ol>
  <form class="add-form" v-if="isEditorMode" @submit.prevent="pushItem()" @click.stop>
    <input type="text" :placeholder="placeholder" v-model="inputValue" />
    <button type="submit" @click.stop>add</button>
  </form>
</template>

<style lang="scss" scoped>
.editable-item {
  display: flex;
  justify-content: start;
  align-items: center;

  button {
    @include iconize-text;

    color: $shadow;
    background-color: transparent;

    box-shadow: none;

    padding: 0px;
    margin-left: -5px;
    margin-right: 5px;

    font-size: 21px;
  }
}

.marked {
  li {
    font-size: 16px;
    font-style: italic;

    &::before {
      content: '–';
      padding-right: 10px;
    }
  }
}

.add-form {
  display: flex;
  justify-content: space-between;
  align-items: center;

  grid-column: 1/4;

  color: $gray-font;
  background-color: transparent;

  input {
    background-color: inherit;

    width: 100%;

    font-size: 16px;
    font-style: italic;
  }

  button {
    @include iconize-text;

    color: $shadow;
    background-color: inherit;

    box-shadow: none;

    padding: 4px;
  }

  &::before {
    content: '–';
    padding-right: 10px;
  }
}
</style>
