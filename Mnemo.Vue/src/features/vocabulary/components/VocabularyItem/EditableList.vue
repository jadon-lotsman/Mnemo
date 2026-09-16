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
  const str = inputValue.value

  if (str == '') return

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
    emitChanges()
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

  li {
    text-wrap-mode: wrap;
    text-wrap-style: stable;
    word-break: break-all;
  }

  button {
    @include iconize;
    margin-right: 5px;
    margin-left: -5px;

    box-shadow: none;
    background-color: transparent;

    padding: 0px;

    color: $shadow-color;

    font-size: 21px;
  }
}

.marked {
  li {
    font-style: italic;
    font-size: 16px;

    &::before {
      padding-right: 10px;
      content: '–';
    }
  }
}

.add-form {
  display: flex;

  grid-column: 1/4;
  justify-content: space-between;
  align-items: center;
  background-color: transparent;

  color: $text-secondary;

  input {
    background-color: inherit;

    width: 100%;
    font-style: italic;

    font-size: 16px;
  }

  button {
    @include iconize;

    box-shadow: none;
    background-color: inherit;

    padding: 4px;

    color: $shadow-color;

    font-size: 24px;
  }

  &::before {
    padding-right: 10px;
    content: '–';
  }
}
</style>
