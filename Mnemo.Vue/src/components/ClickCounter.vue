<script setup lang="ts">
import { ref } from 'vue'

const count = ref<number>(0)

const increment = function () {
  count.value++
}

const decrement = function () {
  count.value--
}
</script>

<template>
  <h1>Счетчик {{ count }}</h1>
  <button @click="increment">Прибавить</button>
  <button @click="decrement">Отнять</button>
</template>

<style scoped></style>
