# Semáforo Inteligente

O objetivo deste projeto consiste na criação de um sistema que consiga gerir um conjunto de semáforos de forma inteligente, auxiliado pela dashboard K8055.

Este sistema deve possuir:
> Modo normal (x em x segundos alternar a cor dos sinais)
>
> Modo noturno (a partir de uma certa hora tornar todos intermitentes)
>  
> Botão da Polícia (todos os semáforos ficarem intermitentes)


## Modo Normal

Como dito anteriormente de x em x segundos, a cor dos semáforos deve alternar, mas há que ter em conta esta modificação. A mudança de cores não deve ser feita imediatamente nos 3 semáforos simultaneamente, visto que poderia levar ao aparecimento de inúmeros acidentes rodoviários.

Deste modo, sugiro a utilização de uma taxa de _delay_.

Imagine-se o caso seguinte:

- Semáforo A encontra-se VERDE;
- Semáforo B encontra-se VERMELHO;
- Semáforo D encontra-se VERDE;

Neste caso em concreto, os veículos que estejam no local do desvio podem-se movimentar, mas os peões também poderam atravessar a passadeira! Caso passe o tal valor x (segundos), os veículos que se encontram estagnados pela cor do semáforo B têm de andar, e os outros pararem!

Segue-se as seguintes ações:

1. Passar para amarelo o semáforo A
2. Passar para vermelho o semáforo A e D
3. Delay de 1.5 segundos (por exemplo) 
4. Passar para verde o semáforo B

Ou seja, **ANTES DE QUALQUER SEMÁFORO PASSAR PARA VERDE DEVE HAVER UM DELAY!**

## Restrições Existentes entre Semáforos

Deve haver um fio condutor lógico.
Quando se inicia o programa, como estão os semáforos?

1. Verificar a hora da máquina! (Caso esteja entre a meia-noite e as 06:00h devem estar intermitentes).
2. Colocar o semáforo B a VERDE, semáforo A e D a VERMELHO.
3. Colocar uma flag indicadora do ciclo em que estamos! (IMPORTANTE).
4. Começar o timer. 
5. Após o timer ser ultrapassado.
6. Colocar o semáforo B a AMARELO, Semáforo A e D mantêm a cor VERMELHA.
7. Passar o semáforo B a VERMELHO, ESPERAR 1.5s e COLOCAR OS OUTROS DOIS SEMÁFOROS A VERDE.
8. Mudar o valor da flag indicadora do ciclo em que passamos a estar! (IMPORTANTE).
9. Começar novamente o timer.
10. Após o timer se ultrapassado.
11. Colocar os Semáforos A e D a AMARELO, Semáforo B mantém VERMELHO.
12. Colocar Semáforos A e D a VERMELHO, ESPERAR 1.5s e COLOCAR O SEMÁFORO B A VERDE.
13. Voltar ao 3


De seguida serão apresentadas algumas restrições que possam existir entre semáforos!

> Caso o Semáforo B esteja VERDE, o A e o D estão VERMELHOS.
> Caso o Semáforo B esteja VERMELHO, o A e o D estão VERDES.
> 