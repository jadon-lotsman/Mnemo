<template>
  <article class="entry-skeelton">
    <header>
      <div class="some-text"></div>

      <div class="some-text"></div>

      <div class="list">
        <div class="some-text"></div>
        <div class="some-text"></div>
        <div class="some-text"></div>
      </div>
    </header>

    <footer>
      <div class="list"></div>
    </footer>
  </article>
</template>

<style lang="scss" scoped>
@keyframes pulse {
  0% {
    opacity: 0.5;
  }
  50% {
    opacity: 0.8;
  }
  100% {
    opacity: 0.5;
  }
}

.entry-skeelton {
  display: flex;
  flex-direction: column;
  align-items: stretch;
  cursor: default;
  box-shadow: 5px 5px 0px $shadow-color;
  border-radius: 12px;

  background-color: $surface-secondary;
  min-width: 370px;

  max-width: 470px;

  .some-text {
    animation: pulse 1.2s infinite ease-in-out;
    margin-bottom: 4px;
    border-radius: 4px;
    background: $surface-secondary;
    width: 75%;
    height: 14px;
  }

  .list {
    display: flex;
    flex-direction: column;

    margin-top: -2px;
  }

  header {
    display: grid;
    grid-template-columns: 30% 30% 40%;

    border-radius: 12px;
    background-color: $surface-primary;

    padding: 8px 6px 6px 16px;
  }

  footer {
    padding: 17px;
  }
}
</style>
