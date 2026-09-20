<script setup lang="ts">
defineProps<{
  text: string
}>()
</script>

<template>
  <div class="module-text">{{ text }}</div>
</template>

<style lang="scss" scoped>
.module-text {
  margin: 6px 0 0 10px;
  color: $text-secondary;
  font-size: 15px;
}
</style>
