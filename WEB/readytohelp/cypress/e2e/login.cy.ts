describe('Página de Login', () => {
  
  beforeEach(() => {
    // Visits the login page before each test
    cy.visit('/login');
  });

  it('deve apresentar o formulário corretamente', () => {
    // Checks if the title exists
    cy.contains('h2', 'Welcome back!').should('be.visible');
    
    // Checks fields
    cy.get('input[placeholder="Email"]').should('be.visible');
    cy.get('input[placeholder="Password"]').should('be.visible');
    
    // Checks button
    cy.get('button[type="submit"]').should('contain', 'Login');
  });

  it('should show error with invalid credentials', () => {
    // Fill in data
    cy.get('input[placeholder="Email"]').type('errado@teste.com');
    cy.get('input[placeholder="Password"]').type('senhaerrada');
    
    // Click
    cy.get('button[type="submit"]').click();
    
    // Check Error Toast (class .alert-danger)
    cy.get('.alert-danger').should('be.visible')
      .and('contain.text', 'Login failed'); // or 'Error'
  });

  it('should successfully login (Mock)', () => {
    // Intercept the API request and respond with fake success
    cy.intercept('POST', '**/auth/login/web', {
      statusCode: 200,
      body: '"fake-jwt-token-123"' // Valid JSON string
    }).as('loginRequest');

    // Fill in data
    cy.get('input[placeholder="Email"]').type('admin@readytohelp.com');
    cy.get('input[placeholder="Password"]').type('123456');
    
    // Click
    cy.get('button[type="submit"]').click();

    // Wait for the request to happen
    cy.wait('@loginRequest');

    // Check redirection to the map
    cy.url().should('include', '/map');
  });
});