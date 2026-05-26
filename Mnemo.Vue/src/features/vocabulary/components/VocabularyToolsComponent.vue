<script setup lang="ts">
import { ref } from 'vue'

const props = defineProps({
  isLoading: Boolean,
})

const emit = defineEmits<{
  (e: 'search', query: string): void
}>()

const searchQuery = ref('')

function onSubmit() {
  emit('search', searchQuery.value)
}
</script>

<template>
  <div class="tools-container">
    <form class="search-form" @submit.prevent="onSubmit">
      <input
        v-model="searchQuery"
        type="search"
        :disabled="props.isLoading"
        :placeholder="props.isLoading ? 'Loading...' : 'Search...'"
      />
      <button type="submit" class="small-button" :disabled="props.isLoading">
        <span>arrow_forward</span>
      </button>
    </form>

    <button type="button" class="small-button" :disabled="props.isLoading">
      <span>add</span>
    </button>
  </div>
</template>

<style lang="scss" scoped>
.tools-container {
  display: flex;
  flex-wrap: nowrap;

  .small-button {
    @include lift();

    position: relative;

    color: $shadow;
    background-color: $plane-gray;

    span {
      position: absolute;

      top: 8px;
      left: 8px;

      font-size: 24px;
      line-height: 1;
      font-family: $iconizeFont;
    }
  }

  .search-form {
    @include lift();

    display: flex;
    justify-content: space-between;
    background-color: $clear-white;
    box-shadow: 5px 5px 0px $shadow;
    width: 100%;

    border-radius: 12px;
    margin-bottom: 15px;

    margin-right: 10px;

    input {
      border: 3px solid $plane-gray;
      border-radius: 12px 0px 0px 12px;
      border-right: none;
      background-color: inherit;

      padding: 8px 12px;
      width: 100%;

      font-size: 15px;
      font-weight: 500;
    }

    button {
      @include lift(0, 0);

      margin-left: -10px;
    }
  }
}
</style>
