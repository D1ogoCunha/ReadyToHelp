describe('Página de Login', () => {
  
  beforeEach(() => {
    // Visita a página de login antes de cada teste
    cy.visit('/login');
  });

  it('deve apresentar o formulário corretamente', () => {
    // Verifica se o título existe
    cy.contains('h2', 'Welcome back!').should('be.visible');
    
    // Verifica campos
    cy.get('input[placeholder="Email"]').should('be.visible');
    cy.get('input[placeholder="Password"]').should('be.visible');
    
    // Verifica botão
    cy.get('button[type="submit"]').should('contain', 'Login');
  });

  it('deve mostrar erro com credenciais inválidas', () => {
    // Preencher dados
    cy.get('input[placeholder="Email"]').type('errado@teste.com');
    cy.get('input[placeholder="Password"]').type('senhaerrada');
    
    // Clicar
    cy.get('button[type="submit"]').click();
    
    // Verificar Toast de Erro (classe .alert-danger)
    cy.get('.alert-danger').should('be.visible')
      .and('contain.text', 'Login failed'); // ou 'Error'
  });

  it('deve fazer login com sucesso (Mock)', () => {
    // Interceptar o pedido para a API e responder sucesso falso
    cy.intercept('POST', '**/auth/login/web', {
      statusCode: 200,
      body: '"fake-jwt-token-123"' // String JSON válida
    }).as('loginRequest');

    // Preencher
    cy.get('input[placeholder="Email"]').type('admin@readytohelp.com');
    cy.get('input[placeholder="Password"]').type('123456');
    
    // Clicar
    cy.get('button[type="submit"]').click();

    // Esperar que o pedido aconteça
    cy.wait('@loginRequest');

    // Verificar redirecionamento para o mapa
    cy.url().should('include', '/map');
  });
});