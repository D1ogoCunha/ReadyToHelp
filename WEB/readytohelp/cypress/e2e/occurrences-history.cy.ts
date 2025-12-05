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
    // Intercept the API call to return mock data
    cy.intercept('GET', '**/api/occurrence*', {
      statusCode: 200,
      body: mockOccurrences
    }).as('getHistory');

    //Visit the occurrences history page with authentication
    cy.visit('/occurrences/history', {
      onBeforeLoad: (window) => {
        window.localStorage.setItem('token', 'fake-jwt-token'); 
        
        // window.localStorage.setItem('currentUser', JSON.stringify({ name: 'Tester', role: 'ADMIN' }));
      }
    });

    cy.url().should('include', '/occurrences/history');

    // Wait for the API call to complete
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
    // Search 1
    cy.get('.filter-input').type('Historic{enter}');
    cy.wait('@getHistory').then((interception) => {
      expect(interception.request.url).to.include('filter=Historic');
    });

    // Search 2
    cy.get('.filter-input').clear().type('Fire{enter}');
    cy.wait('@getHistory'); // Wait for reload

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

  it('should show error message when API fails', () => {
    // Force a failed API response
    cy.intercept('GET', '**/api/occurrence*', {
      statusCode: 500,
      body: { message: 'Internal Server Error' }
    }).as('getHistoryError');

    // Reload to trigger the failed request
    cy.reload();
    cy.wait('@getHistoryError');

    // Verify if the error message appears
    cy.get('.alert.alert-danger')
      .should('be.visible')
      .and('contain.text', 'It was not possible to load the history.');
  });

  it('should apply complex filters (Type, Priority, Dates)', () => {
    // 1. Filter by Type
    cy.get('select').eq(0).select('FOREST_FIRE'); 
    // The component reloads when the filter changes
    cy.wait('@getHistory');
    
    // 2. Filter by Priority
    cy.get('select').eq(1).select('HIGH');
    cy.wait('@getHistory');

    // 3. Filter by Dates (Start Date)
    // Define a date that excludes the item "Old Fire Incident" (2023-02-20) but keeps the other if adjusted
    cy.get('input[type="date"]').first().type('2023-01-01');
    cy.wait('@getHistory');

    // Verify if the inputs maintain their values
    cy.get('select').eq(0).should('have.value', 'FOREST_FIRE');
    cy.get('select').eq(1).should('have.value', 'HIGH');
  });

  it('should enforce "CLOSED" status filter client-side', () => {
    // Mock that returns a mixed ACTIVE occurrence (which should not appear in the history list)
    const mixedResponse = {
      items: [
        { id: 900, title: 'Active Fire', status: 'ACTIVE', type: 'FOREST_FIRE', priority: 'HIGH', creationDateTime: '2023-10-01', reportCount: 1 },
        { id: 901, title: 'Closed Flood', status: 'CLOSED', type: 'FLOOD', priority: 'LOW', creationDateTime: '2023-10-02', reportCount: 2 }
      ],
      totalItems: 2
    };

    cy.intercept('GET', '**/api/occurrence*', {
      statusCode: 200,
      body: mixedResponse
    }).as('getMixedHistory');

    cy.reload();
    cy.wait('@getMixedHistory');

    // The CLOSED occurrence should be visible
    cy.contains('Closed Flood').should('be.visible');
    
    // The ACTIVE occurrence should be filtered out by the component code and not displayed
    cy.contains('Active Fire').should('not.exist');
  });

  it('should toggle sort order when clicking the same header', () => {
    // Click on 'Start Date' (default is desc, should change to asc)
    cy.contains('th', 'Start Date').click();
    cy.wait('@getHistory').then((interception) => {
      expect(interception.request.url).to.include('sortOrder=asc');
    });
    
    // Verify visual indicator
    cy.contains('th', 'Start Date').find('.sort-ind').should('contain.text', '▲');

    // Click again (should revert to desc)
    cy.contains('th', 'Start Date').click();
    cy.wait('@getHistory').then((interception) => {
      expect(interception.request.url).to.include('sortOrder=desc');
    });

    // Verify visual indicator
    cy.contains('th', 'Start Date').find('.sort-ind').should('contain.text', '▼');
  });
});