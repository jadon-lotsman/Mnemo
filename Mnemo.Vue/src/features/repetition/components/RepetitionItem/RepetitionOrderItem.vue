<script setup lang="ts">
import { ref } from 'vue'
const props = defineProps<{
  modelValue: string
  parts: string[]
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', order: string): void
}>()

const allowParts = ref<string[]>(props.parts)
const resultParts = ref<string[]>([])

function pushPart(str: string, index: number) {
  const part = allowParts.value[index]
  if (part) {
    allowParts.value.splice(index, 1)
    resultParts.value.push(part)

    emitChanges()
  }
}

function removePart(str: string, index: number) {
  const part = resultParts.value[index]
  if (part) {
    resultParts.value.splice(index, 1)
    allowParts.value.push(part)

    emitChanges()
  }
}

function emitChanges() {
  emit('update:modelValue', resultParts.value.join(' '))
}
</script>

<template>
  <div class="order-input">
    <header>
      <span
        class="part"
        v-for="(part, index) in resultParts"
        :key="index"
        v-text="part"
        @click="removePart(part, index)"
      />
    </header>
    <div class="splitter"></div>
    <footer v-if="allowParts.length > 0">
      <span
        class="part"
        v-for="(part, index) in allowParts"
        :key="index"
        v-text="part"
        @click="pushPart(part, index)"
      />
    </footer>
  </div>
</template>

<style lang="scss" scoped>
.order-input {
  display: flex;
  justify-content: start;
  flex-direction: column;

  .splitter {
    background-color: $plane-gray;

    height: 3px;
    margin-top: 5px;

    border-radius: 1px;
  }

  footer {
    margin-top: 10px;
  }

  footer,
  header {
    display: flex;
    justify-content: start;
    flex-direction: row;
    flex-wrap: wrap;

    gap: 5px;

    min-height: 10px;

    .part {
      cursor: pointer;
      user-select: none;

      display: flex;
      align-items: center;

      background-color: $plane-gray;

      padding: 3px 6px;
      height: 25px;

      border-radius: 8px;

      line-height: 1;

      font-size: 16px;
    }
  }
}
</style>
