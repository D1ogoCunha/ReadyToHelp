describe('Occurrences History Page', () => {

  const mockOccurrences = {
    items: [
      {
        id: 101,
        title: 'Historic Flood',
        type: 'FLOOD',
        status: 'CLOSED',
        priority: 'HIGH',
        creationDateTime: '2023-01-15T10:00:00',
        endDateTime: '2023-01-16T12:00:00',
        reportCount: 10
      },
      {
        id: 102,
        title: 'Old Fire Incident',
        type: 'FIRE',
        status: 'CLOSED',
        priority: 'MEDIUM',
        creationDateTime: '2023-02-20T14:30:00',
        endDateTime: '2023-02-20T18:00:00',
        reportCount: 3
      }
    ],
    totalItems: 2,
    totalPages: 1
  };

  beforeEach(() => {
    // 1. Intercept mais abrangente (singular ou plural)
    // Cobre: /api/occurrences?page=1 E /api/occurrence?page=1
    cy.intercept('GET', '**/api/occurrence*', {
      statusCode: 200,
      body: mockOccurrences
    }).as('getHistory');

    // 2. Visitar a página simulando autenticação
    cy.visit('/occurrences/history', {
      onBeforeLoad: (window) => {
        // TENTA DESCOBRIR A CHAVE CORRETA NO TEU BROWSER (Application -> Local Storage)
        // Exemplos comuns: 'token', 'access_token', 'currentUser', 'auth'
        window.localStorage.setItem('token', 'fake-jwt-token'); 
        
        // Se a tua app guardar o user num objeto separado, adiciona também:
        // window.localStorage.setItem('currentUser', JSON.stringify({ name: 'Tester', role: 'ADMIN' }));
      }
    });

    // 3. Verificar se NÃO fomos redirecionados para o login
    // Se esta linha falhar, o problema é o Auth Guard da tua app
    cy.url().should('include', '/occurrences/history');

    // 4. Agora sim, esperar pelo pedido
    cy.wait('@getHistory');
  });

  it('should display the history page structure correctly', () => {
    cy.get('.um-title').should('contain.text', 'Occurrences History');
    cy.get('.filter-input').should('exist');
    cy.get('.toolbar-select').should('have.length', 2);
    cy.get('button').contains('Reset').should('exist');
  });

  it('should render the list of closed occurrences', () => {
    cy.get('table.um-table tbody tr').should('have.length', 2);
    cy.get('tbody tr').first().within(() => {
      cy.contains('#101');
      cy.contains('Historic Flood');
      cy.contains('Flood');
      cy.contains('High');
      cy.get('.text-center').should('contain.text', '10');
    });
  });

  it('should handle search filtering', () => {
    // Pesquisa 1
    cy.get('.filter-input').type('Historic{enter}');
    cy.wait('@getHistory').then((interception) => {
      expect(interception.request.url).to.include('filter=Historic');
    });

    // Pesquisa 2
    cy.get('.filter-input').clear().type('Fire{enter}');
    cy.wait('@getHistory'); // Esperar reload

    cy.get('tbody tr').should('contain.text', 'Old Fire Incident');
    cy.get('tbody tr').should('not.contain.text', 'Historic Flood');
  });

  it('should handle sorting interactions', () => {
    cy.contains('th', 'ID').click();
    cy.wait('@getHistory').then((interception) => {
      expect(interception.request.url).to.include('sortBy=Id');
    });

    cy.contains('th', 'Title').click();
    cy.wait('@getHistory').then((interception) => {
      expect(interception.request.url).to.include('sortBy=Title');
    });
    
    cy.contains('th', 'Title').should('have.class', 'sorted');
  });

  it('should navigate to details page on click', () => {
    cy.get('tbody tr').first().find('a[title="View details"]').click();
    cy.url().should('include', '/occurrence/101');
  });

  it('should reset filters when Reset button is clicked', () => {
    // Apply a filter
    cy.get('.filter-input').type('actual{enter}');
    
    // Click Reset
    cy.get('button').contains('Reset').click();

    // Verify input is empty
    cy.get('.filter-input').should('have.value', '');
    
    // Verify API called (reloading the page)
    cy.wait('@getHistory');
  });

  it('should display empty state when no data is found', () => {
    cy.intercept('GET', '**/api/occurrence*', {
      statusCode: 200,
      body: { items: [], totalItems: 0 }
    }).as('getEmptyHistory');

    cy.get('button').contains('Reset').click();
    cy.wait('@getEmptyHistory');

    cy.get('.empty-state').should('be.visible');
    cy.get('.empty-state h5').should('contain.text', 'No closed occurrences found');
    cy.get('table.um-table').should('not.exist');
  });

});